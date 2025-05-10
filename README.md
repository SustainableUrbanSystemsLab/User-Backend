# User Backend

Eddy3D is an AutoCAD extension designed to provide advanced simulations and visualization for environmental dynamics research. The goal of this project is to expand the backend, primarily adding the functionalities of (1) User Metrics, and (2) Quotas, while building upon the current user data infrastructure and enhancing the current API.

[Licenced under MIT ](LICENSE)

---

[Installation Guide (PDF)](Backend_Installation_Guide.pdf)

[Detailed Design Document (PDF)](Detailed_Design_Document.pdf)

# Release Notes
## Version 1.0

### Features
- **Admin Dashboard**:  
  - Admin dashboard to visualize MongoDB database via Metabase
  - run_metabase script

- **Communities**:  
  - Communities support and tracking for users upon registration and login for different services
  - Current Communities:
    - Eddy3D
    - Urbano

- **Email Verification Page**:  
  - Webpage for email verification

- **Further Stability**:  
  - Additional stability for:
    - Tokens
    - Metrics
    - Secrets/Environment Variables
    - Exception Handling

### Bug Fixes
- Reverted problematic userId standardization
- Verification issues

### Known Issues
- Minimal/no email verification fail state functionality and webpage 

### Features
- **Production Build**:  
  - Implemented Correct Build Settings

- **User Base Transfer**:  
  - Built and executed routine to bring all previous Eddy3D users into new database
  - Included special flag for these past users

- **Initial Live Production Developments**:  
  - Integrated into Eddy3D frontend
  - Implemented new live production database
  - Implemented live production API host

### Bug Fixes
- Fixed Verification and Email services to rely on userID.

### Known Issues
- Issues with the verification system not correctly updating the database for verified users

### Features
- **Exception Handling**:  
  - Auth Controller
  - Admin Controller
  - Wallet Controller
  - Redundant PUTs

- **Controller Authorization (Complete)**:  
  - Admin Controller
  - Wallet Controller

- **Wallet Standarization**:  
  - Wallet Controller and functionality standardized

- **DTO Standarization**:  
  - All DTO's Standardized

- **User Calls Standarization**:  
  - Calls for User standardized to use UserId

- **Secrets and Environment Variables (Update)**:  
  - MongoDB Connection String

### Bug Fixes
- (Added Exception Handling)

### Known Issues
- Verification and Email services rely on UserName to some degree still.

### Features
- **Account Infrastructure (Stable)**:  
  - Account Data Layout
    - Personal Data
      - Username/Email
      - Password
      - Id
    - Personal Metrics
      - Total Simulations Run
      - Daily Logins
    - Quota Token Wallet 
  - Account Type Hierarchy
    - User
    - Admin
    - VIP
  - Email Verification
    - Verification Link Email
  - Account Mutation Functionality
    - Password Change
    - Email Change
    - OTP Emails
  - API Calls
    - Ready for front-end, testing in Hoppscotch

- **Administrative Functionalities**:  
  - User Management
    - Account Deactivation
    - Account Reactivation
  - Quota Management
    - Quota Token Issuing
    - View Token Wallets

- **Secrets and Environment Variables (Initial)**:  
  - Email Strings

### Bug Fixes
- Removed Saving Login Date for Registering

### Known Issues
- Admin Controller not completely covered by authorization
- No exception handling for Setting Roles
- No handling redudant PUTs
- DTOs for requests need refactoring
- Need to standardize how users are fetched, id vs name
- Wallet functions/calls need to be standardized

### Features
- **User & Simulation Metrics**:  
  - (Temporal) Login Metrics
    - Day
    - Week
    - Month
    - Year
  - (Temporal) Unique Login Metrics
    - Day
    - Week
    - Month
    - Year
  - (Temporal) Registration Metrics 
    - Day
    - Week
    - Month
    - Year
  - (Temporal & Type-Specific) Simulation Metrics
    - (Simulation-Type) Day
    - (Simulation-Type) Week
    - (Simulation-Type) Month
    - (Simulation-Type) Year

- **Simulation Quotas and Quota Tokens**:  
  - User Quotas
    - Validated by token wallet
    - Transactions of tokens for simulations
  - (Type-Specific) Quota Tokens
    - (Simulation) Type
    - (Token) Life
  - Quota Token Wallets
    - Per User Account
    - Tied to user BSON id
  - Quota Token Transactions
    - Get Wallet Balance
    - Add to Wallet
    - Remove from Wallet
    - Request Token
    - Verify Token Response

### Bug Fixes
- None

### Known Issues
- Login date stored when a user registers causes the registered user to be counted as a unique login user

### Features
- **Basic User Data**:  
  - User registration and login
  - User verification disabled

- **Basic Metrics**:  
  - A basic foundation for user metrics
  - Tracks successful logins
  - Auto-initialization check of metrics model

### Bug Fixes
- This is the initial build

### Known Issues
- No error handling for metrics api calls
- Strange delay/issue with daily login incrementing sometimes
- HTTPS resolve warnings

---

# Technology Stack

- **Platforms**:  
  - Windows
  - Mac
  - Linux

- **Frameworks**:  
  - Eddy3D core engine (proprietary).  
  - .NET 7.0.20.  
  - MongoDB

- **Programming Languages**:  
  - C#
  - .NET
  - MongoDB

- **Data Storage**:  
  - MongoDB
    - User data
    - Metrics data  
