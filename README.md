# Urbano Backend

## JDD Branch

## Eddy3D Backend

Eddy3D is an autoCAD extension designed to provide advanced simulations and visualization for environmental dynamics research. The goal of this project is to expand the backend, primarily adding the functionalities of (1) User Metrics, and (2) Quotas, while building upon the current user data infrastructure and enhancing the current API.

[Licenced under MIT ](LICENSE)

---

# Release Notes

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

---

## Sendgrid API Key (account under audit)

> `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`

---

## MongoDB on Atlas
Connection string: 

> `mongodb+srv://admin:Goofiness-Battle4-Prodigal@urbano-backend-users.syw1bpp.mongodb.net/?retryWrites=true&w=majority`

[Web Login](https://cloud.mongodb.com/v2/652813be4ab4f40f3379b837#/overview)  

---

## Hoppscotch Workspace unit tests

[Link](https://hoppscotch.io)

---