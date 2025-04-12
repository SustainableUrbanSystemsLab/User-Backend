import csv
import os
import sys
from dotenv import load_dotenv
from pymongo import MongoClient, errors
from bson import ObjectId
from datetime import datetime

# Load environment variables from .env in the current project root
load_dotenv()

mongo_uri = os.getenv("MONGO_CONNECTIONSTRING")
db_name = "UrbanoStore"  # Update if needed

# Attempt to connect to MongoDB
try:
    client = MongoClient(mongo_uri, serverSelectionTimeoutMS=5000)
    client.server_info()  # Force connection check
except errors.ServerSelectionTimeoutError as err:
    print(f"Error connecting to MongoDB: {err}")
    sys.exit(1)

db = client[db_name]
users_collection = db["Users"]

# Secure hashed password - must match format used in your backend
SECURE_HASHED_PASSWORD = (
    "B2304F1285751CD3297155CB6D7C1ACEBA6DABEC6D60F01A1701CE0E03515463A5E2D4EC8F6D215CA04C340E167F1B3F1907BC4EBF14E67DB9BB37683C6B05E2"
)

# Defaults based on your sample
DEFAULT_DATE = datetime(2025, 1, 27, 5, 0, 0)
DEFAULT_LAST_LOGIN_DATE = datetime(2025, 1, 28, 3, 22, 35, 790000)

# List of CSV file paths to process (adjust paths as needed)
CSV_FILES = [
    os.path.join("..", "Misc", "eddy3d.csv"),
    os.path.join("..", "Misc", "existing_contacts_export-brevo.csv")
]

def process_csv(csv_path):
    print(f"Processing CSV file: {csv_path}")
    row_count = 0
    try:
        with open(csv_path, newline='', encoding='utf-8') as csvfile:
            reader = csv.DictReader(csvfile)
            for row in reader:
                row_count += 1
                try:
                    # Convert keys and values to uppercase and strip whitespace for case-insensitive lookup.
                    # (We leave the values as-is, but trim them.)
                    row_uc = {k.strip().upper(): v.strip() for k, v in row.items() if k is not None and v is not None}
                    email = row_uc.get("EMAIL", "")
                    if not email:
                        continue  # Skip rows without a valid email
                    email = email.lower()

                    # Map fields from the CSV.
                    extra_fields = {
                        "FirstName": row_uc.get("FIRSTNAME", "N/A"),
                        "LastName": row_uc.get("LASTNAME", "N/A"),
                        "NameOfInstitution": row_uc.get("NAME_OF_INSTITUTION", "N/A"),
                        "CurrentProfession": row_uc.get("CURRENT_PROFESSION", "N/A"),
                        "CurrentProfessionIfOther": row_uc.get("CURRENT_PROFESSION_IF_OTHER", "N/A"),
                        "IntendToUseEddy3dAs": row_uc.get("I_INTEND_TO_USE_EDDY3D_AS", "N/A"),
                        "AddedTime": row_uc.get("ADDED_TIME", "N/A"),
                        "ModifiedTime": row_uc.get("MODIFIED_TIME", "N/A"),
                        "Platform": row_uc.get("PLATFORM", "N/A")
                    }

                    # Define old/unwanted fields to remove (the keys here should match exactly those stored previously)
                    unwanted = {"currentprogression", "currentprogressionifother", "intendtousebeyond15"}
                    unset_fields = {}
                    # Look at the existing document (if any) to identify keys to remove (comparing lowercased keys)
                    existing = users_collection.find_one({"UserName": email})
                    if existing:
                        for key in existing.keys():
                            if key.lower() in unwanted:
                                unset_fields[key] = ""
                    
                    # Build the update document.
                    update_doc = {"$set": extra_fields}
                    if unset_fields:
                        update_doc["$unset"] = unset_fields

                    if existing:
                        users_collection.update_one({"UserName": email}, update_doc)
                        print(f"Updated existing user: {email}")
                    else:
                        user_doc = {
                            "_id": ObjectId(),
                            "UserName": email,
                            "Password": SECURE_HASHED_PASSWORD,
                            "Organization": None,
                            "Verified": True,
                            "AttemptsLeft": 4,
                            "MaxAttempts": 4,
                            "AffiliationType": None,
                            "Role": "USER",
                            "Date": DEFAULT_DATE,
                            "LastLoginDate": DEFAULT_LAST_LOGIN_DATE,
                            "MigratedUser": True
                        }
                        user_doc.update(extra_fields)
                        users_collection.insert_one(user_doc)
                        print(f"Migrated new user: {email}")

                except Exception as row_err:
                    print(f"Error processing row {row_count}: {row_err}")

    except FileNotFoundError:
        print(f"CSV file not found: {csv_path}")
    except Exception as csv_err:
        print(f"Error reading CSV file: {csv_err}")
    print(f"Finished processing {csv_path}. Total rows processed: {row_count}")

if __name__ == "__main__":
    for csv_file in CSV_FILES:
        process_csv(csv_file)

