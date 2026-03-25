## 🔧 Local Setup

### 1. Start Infrastructure
docker compose up -d

### 2. Setup API Secrets
cd BoardingBase.Api

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=boardingbase;Username=mondz;Password=monlitz123"

dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379"

### 3. Run API
dotnet run

### 4. Run Frontend
cd boardingbase-web
npm install
npm run dev