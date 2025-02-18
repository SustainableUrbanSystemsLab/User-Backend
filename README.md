# Urbano Backend

## JDD Branch

## Eddy3D Backend

Eddy3D is an autoCAD extension designed to provide advanced simulations and visualization for environmental dynamics research. The goal of this project is to expand the backend, primarily adding the functionalities of (1) User Metrics, and (2) Quotas, while building upon the current user data infrastructure and enhancing the current API.

[Licenced under MIT ](LICENSE)

---

# Release Notes

## Version 0.2.0

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
  - Accout Type Hierarchy
    - User
    - Admin
    - VIP
  - Email Verification
    - Verification Link Email
  - Account Mutation Functionality
    - Passwrod Change
    - Email Change
    - OTP Emails
  - API Calls
    - Ready for front end, testing in Hoppscotch
- **Administrative Functionalities**:  
  - User Management
    - Account Deactivation
    - Account Reactivation
  - Quota Management
    - Quota Token Issuing
    - View Token Wallets

- **Secrets and Enviornment Variables (Initial)**:  
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

## Version 0.1.0

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
    - (Simulatoin) Type
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
- Login date stored when a user registers causes registered user to be counted as a unique login user

## Version 0.0.0

### Features
- **Basic User Data**:  
  - User registration and login
  - User verification dissabled

- **Basic Metrics**:  
  - A basic foundation for user metrics
  - Tracks successfull logins
  - Auto-initialization check of metrics model

### Bug Fixes
- This is the initial build

### Known Issues
- No error handeling for metrics api calls
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