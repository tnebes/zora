# zora 🇬🇧

## Instructions
- Install and configure MSSQL
- Install dotnet (9.x.x) core
- Install nodejs (v22.x.x) and npm (v10.x.x)
- Install angular  (19.x.x) cli (v14.x.x)
- Clone the repository
- Run the `db.sql` script in the MSSQL database using root user
- Run `startup.sql` with the changed password in the MSSQL database using root user
- Setting up secrets
   - `cd zora/app/`
   - `dotnet user-secrets init`
   - `dotnet user-secrets set "Zora:ConnectionString" "Server=.;Database=zora;User Id=zora_service;Password=your_strong_password_here;TrustServerCertificate=True;"`
   - `dotnet user-secrets set "Zora:SecretKey" "some_secret_key_that_should_be_very_long_and_random_1234567890"`
   - create `C:\Users\YOURUSERNAME\AppData\Roaming\ASP.NET\https` folder
   - create `C:\Users\YOURUSERNAME\.aspnet\https` folder
- `GET /api/v1/seed` for initial data seeding (only for development purposes)
    - in `DataSeeder.cs` file, change the number of users, programs, projects, tasks, roles, permissions, assets

## Acronym
Platform for Work Organisation (Platforma **z**a **o**rganizaciju **ra**da)

## Description

Zora is a platform for work organisation. It is a web application that allows users to create and manage tasks, projects, and teams. It is designed to help users organise their work and collaborate with others.

Zora is built on dotnet core and uses a MSSQL database. It is designed to be easy to use and flexible, with a simple and intuitive user interface.

## Zora Project Management System Features

### User Management & Security

- **Authentication System**
  - User registration with unique usernames and email validation
  - Secure password storage and management
  - User profile management capabilities

- **Access Control**
  - Role-based access control (RBAC) framework
  - Custom role creation and management
  - Binary permission system (4-digit format) for granular access control
  - Work item-specific permission assignments

### Work Item Management

- **Hierarchical Organization**
  - Programs (top-level organization units)
  - Projects (mid-level units, organized under programs)
  - Tasks (atomic work units, organized under projects)

- **Work Tracking Features**
  - Status monitoring and updates
  - Progress tracking with completion percentages
  - Time estimation and actual time logging
  - Start and due date management
  - Version control with timestamp tracking

- **Task Management**
  - Priority level assignment
  - Parent-child task relationships
  - Project association capabilities
  - Task dependency management

- **Project Management**
  - Program association functionality
  - Project manager assignment
  - Multi-task coordination
  - Project resource allocation

### Relationship Management

- **Work Item Relationships**
  - Flexible relationship type definitions
  - Cross-item linking capabilities
  - Relationship creation timestamping
  - Bi-directional relationship tracking

### Asset Management

- **Digital Asset System**
  - File storage and organization
  - Asset metadata management
  - Version control for assets
  - Work item asset associations
  - Multi-asset support per work item

### Reporting & Tracking

- **Progress Monitoring**
  - Time tracking comparisons
  - Completion status updates
  - Assignment tracking
  - Resource utilization monitoring

- **Audit System**
  - Creation and modification timestamps
  - User action logging
  - Version history tracking
  - Change management

### Technical Features

- **Performance Optimization**
  - Indexed work item queries
  - Optimized assignee lookups
  - Task priority filtering
  - Relationship search capabilities
  - Asset association indexing

- **Data Integrity**
  - Referential integrity enforcement
  - Unique constraint validation
  - Permission format validation
  - Required field verification
  - Cross-reference validation

## Technologies

- dotnet core
- MSSQL
- Entity Framework Core
- Angular

---

# zora 🇭🇷

## Akronim
Platforma **z**a **o**rganizaciju **ra**da

## Opis

Zora je platforma za organizaciju rada. Web je aplikacija koja korisnicima omogućuje stvaranje i upravljanje zadacima, projektima i timovima. Dizajnirana je kako bi pomogla korisnicima organizirati svoj rad i surađivati s drugima.

Zora je izgrađena na dotnet core-u i koristi MSSQL bazu podataka. Dizajnirana je da bude jednostavna za korištenje i fleksibilna, s jednostavnim i intuitivnim korisničkim sučeljem.

## Značajke Zora sustava za upravljanje projektima

### Upravljanje korisnicima i sigurnost

- **Sustav autentifikacije**
  - Registracija korisnika s jedinstvenim korisničkim imenima i validacijom e-pošte
  - Sigurno spremanje i upravljanje lozinkama
  - Mogućnosti upravljanja korisničkim profilom

- **Kontrola pristupa**
  - Okvir za kontrolu pristupa temeljen na ulogama (RBAC)
  - Izrada i upravljanje prilagođenim ulogama
  - Binarni sustav dozvola (4-znamenkasti format) za preciznu kontrolu pristupa
  - Dodjela dozvola specifičnih za radne stavke

### Upravljanje radnim stavkama

- **Hijerarhijska organizacija**
  - Programi (organizacijske jedinice najviše razine)
  - Projekti (jedinice srednje razine, organizirane unutar programa)
  - Zadaci (atomske radne jedinice, organizirane unutar projekata)

- **Značajke praćenja rada**
  - Praćenje statusa i ažuriranja
  - Praćenje napretka s postocima završenosti
  - Procjena vremena i bilježenje stvarnog vremena
  - Upravljanje datumima početka i završetka
  - Kontrola verzija s praćenjem vremenske oznake

- **Upravljanje zadacima**
  - Dodjela razine prioriteta
  - Odnosi zadataka roditelj-dijete
  - Mogućnosti povezivanja projekata
  - Upravljanje ovisnostima zadataka

- **Upravljanje projektima**
  - Funkcionalnost povezivanja programa
  - Dodjela voditelja projekta
  - Koordinacija više zadataka
  - Raspodjela projektnih resursa

### Upravljanje odnosima

- **Odnosi radnih stavki**
  - Fleksibilne definicije vrste odnosa
  - Mogućnosti povezivanja među stavkama
  - Vremenske oznake stvaranja odnosa
  - Dvosmjerno praćenje odnosa

### Upravljanje resursima

- **Sustav digitalnih resursa**
  - Pohrana i organizacija datoteka
  - Upravljanje metapodacima resursa
  - Kontrola verzija resursa
  - Povezivanje resursa s radnim stavkama
  - Podrška za više resursa po radnoj stavci

### Izvještavanje i praćenje

- **Praćenje napretka**
  - Usporedbe praćenja vremena
  - Ažuriranja statusa završenosti
  - Praćenje zaduženja
  - Praćenje iskorištenosti resursa

- **Sustav revizije**
  - Vremenske oznake stvaranja i izmjena
  - Bilježenje korisničkih akcija
  - Praćenje povijesti verzija
  - Upravljanje promjenama

### Tehničke značajke

- **Optimizacija performansi**
  - Indeksirani upiti radnih stavki
  - Optimizirano pretraživanje zaduženja
  - Filtriranje prioriteta zadataka
  - Mogućnosti pretraživanja odnosa
  - Indeksiranje povezivanja resursa

- **Integritet podataka**
  - Provođenje referencijalnog integriteta
  - Validacija jedinstvenih ograničenja
  - Validacija formata dozvola
  - Provjera obaveznih polja
  - Validacija unakrsnih referenci

## Tehnologije

- dotnet core
- MSSQL
- Entity Framework Core
- Angular

