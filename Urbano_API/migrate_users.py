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
db_name = "BackendTesting"  # Update if needed

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

# Updated path to your migration CSV
CSV_PATH = os.path.join("..", "Misc", "eddy3d.csv")

def migrate_users(csv_path):
    try:
        with open(csv_path, newline='', encoding='utf-8') as csvfile:
            reader = csv.DictReader(csvfile)
            for row in reader:
                try:
                    # Convert keys to uppercase for case-insensitive lookup
                    row_uc = {k.upper(): v for k, v in row.items()}
                    email = row_uc.get("EMAIL", "").strip().lower()
                    if not email:
                        continue  # Skip rows without a valid email

                    # Map new extra fields from CSV; default to "N/A" if missing.
                    extra_fields = {
                        "FirstName": row_uc.get("FIRSTNAME", "N/A"),
                        "LastName": row_uc.get("LASTNAME", "N/A"),
                        "NameOfInstitution": row_uc.get("NAME_OF_INSTITUTION", "N/A"),
                        "AddedTime": row_uc.get("ADDED_TIME", "N/A"),
                        "ModifiedTime": row_uc.get("MODIFIED_TIME", "N/A")
                    }
                    
                    # Define the old (useless) fields to remove.
                    # Ensure these keys exactly match the field names present in your documents.
                    remove_fields = {
                        "CurrentProgression": "",
                        "CurrentProgressionIfOther": "",
                        "IntendToUseBeyond15": "",
                        "Platform": ""
                    }
                    
                    existing = users_collection.find_one({"UserName": email})
                    if existing:
                        # Update existing document: set new fields and remove the old ones.
                        users_collection.update_one(
                            {"UserName": email},
                            {"$set": extra_fields, "$unset": remove_fields}
                        )
                        print(f"Updated existing user: {email}")
                    else:
                        # Create a new user document with default and extra fields.
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
                    print(f"Error processing row {row}: {row_err}")
    except FileNotFoundError:
        print(f"CSV file not found: {csv_path}")
    except Exception as csv_err:
        print(f"Error reading CSV file: {csv_err}")

if __name__ == "__main__":
    migrate_users(CSV_PATH)

