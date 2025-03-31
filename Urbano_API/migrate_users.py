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
    # Try an operation to check if connection is successful
    client.server_info()
except errors.ServerSelectionTimeoutError as err:
    print(f"Error connecting to MongoDB: {err}")
    sys.exit(1)

db = client[db_name]
users_collection = db["Users"]

# Secure hashed password - must match format used in backend
SECURE_HASHED_PASSWORD = "B2304F1285751CD3297155CB6D7C1ACEBA6DABEC6D60F01A1701CE0E03515463A5E2D4EC8F6D215CA04C340E167F1B3F1907BC4EBF14E67DB9BB37683C6B05E2"

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
                    email = row.get("email", "").strip().lower()
                    if not email:
                        continue

                    existing = users_collection.find_one({"UserName": email})
                    if existing:
                        print(f"User {email} already exists, skipping.")
                        continue

                    user_doc = {
                        "_id": ObjectId(),
                        "UserName": email,
                        "Password": SECURE_HASHED_PASSWORD,
                        "FirstName": row.get("firstName", "Temp"),
                        "LastName": row.get("lastName", "User"),
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
                    
                    try:
                        users_collection.insert_one(user_doc)
                        print(f"Migrated: {email}")
                    except Exception as db_err:
                        print(f"Error inserting {email}: {db_err}")

                except Exception as row_err:
                    print(f"Error processing row {row}: {row_err}")

    except FileNotFoundError:
        print(f"CSV file not found: {csv_path}")
    except Exception as csv_err:
        print(f"Error reading CSV file: {csv_err}")

if __name__ == "__main__":
    migrate_users(CSV_PATH)

