# Urbano Backend

## JDD Branch

# Eddy3D Backend

Eddy3D is an autoCAD extension designed to provide advanced simulations and visualization for environmental dynamics research. The goal of this project is to expand the backend, primarily adding the functionalities of (1) User Metrics, and (2) Quotas, while building upon the current user data infrastructure and enhancing the current API. 

---

# Release Notes

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

# Rationale for Artifact Selection
Version 0.0.0 focused on implementing the essential metrics tracking infrastructure. This decision was made to kickstart the integration of advanced user tracking into Eddy3D. Alongside this comes the re-intergration of basic user data and their functionalities for testing.

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
Related Repos:  
https://github.com/Urbano-io/Urbano-Frontend  