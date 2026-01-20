# Script Minimalista - Roda DENTRO da pasta apex
# Execute este script ja estando dentro do diretorio apex

Write-Host "Criando estrutura do projeto Apex..." -ForegroundColor Cyan

# Verificar se ja existe solution ou projetos
if (Test-Path "Apex.sln") {
    Write-Host "Aviso: Apex.sln ja existe. Removendo..." -ForegroundColor Yellow
    Remove-Item "Apex.sln" -Force
}

# Limpar pastas apex duplicadas se existirem
if (Test-Path "apex") {
    Write-Host "Removendo pasta 'apex' duplicada..." -ForegroundColor Yellow
    Remove-Item -Path "apex" -Recurse -Force
}

# Criar estrutura de pastas
Write-Host "Criando estrutura de pastas..." -ForegroundColor Yellow
$directories = @(
    "src/Apex.Domain",
    "src/Apex.Application", 
    "src/Apex.Infrastructure",
    "src/Apex.Api",
    "src/Apex.Shared",
    "tests/Apex.UnitTests",
    "tests/Apex.IntegrationTests",
    "tests/Apex.ArchitectureTests"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

Write-Host "Pastas criadas com sucesso" -ForegroundColor Green

# Criar Solution
Write-Host "Criando solution..." -ForegroundColor Yellow
dotnet new sln -n Apex

# Criar projetos
Write-Host "Criando projetos..." -ForegroundColor Yellow

Write-Host "  - Apex.Domain" -ForegroundColor Gray
dotnet new classlib -n Apex.Domain -o src/Apex.Domain -f net9.0

Write-Host "  - Apex.Application" -ForegroundColor Gray
dotnet new classlib -n Apex.Application -o src/Apex.Application -f net9.0

Write-Host "  - Apex.Infrastructure" -ForegroundColor Gray
dotnet new classlib -n Apex.Infrastructure -o src/Apex.Infrastructure -f net9.0

Write-Host "  - Apex.Api" -ForegroundColor Gray
dotnet new webapi -n Apex.Api -o src/Apex.Api -f net9.0 --use-controllers

Write-Host "  - Apex.Shared" -ForegroundColor Gray
dotnet new classlib -n Apex.Shared -o src/Apex.Shared -f net9.0

Write-Host "  - Apex.UnitTests" -ForegroundColor Gray
dotnet new xunit -n Apex.UnitTests -o tests/Apex.UnitTests -f net9.0

Write-Host "  - Apex.IntegrationTests" -ForegroundColor Gray
dotnet new xunit -n Apex.IntegrationTests -o tests/Apex.IntegrationTests -f net9.0

Write-Host "  - Apex.ArchitectureTests" -ForegroundColor Gray
dotnet new xunit -n Apex.ArchitectureTests -o tests/Apex.ArchitectureTests -f net9.0

Write-Host "Projetos criados com sucesso" -ForegroundColor Green

# Adicionar a solution
Write-Host "Adicionando projetos a solution..." -ForegroundColor Yellow
dotnet sln Apex.sln add src/Apex.Domain/Apex.Domain.csproj
dotnet sln Apex.sln add src/Apex.Application/Apex.Application.csproj
dotnet sln Apex.sln add src/Apex.Infrastructure/Apex.Infrastructure.csproj
dotnet sln Apex.sln add src/Apex.Api/Apex.Api.csproj
dotnet sln Apex.sln add src/Apex.Shared/Apex.Shared.csproj
dotnet sln Apex.sln add tests/Apex.UnitTests/Apex.UnitTests.csproj
dotnet sln Apex.sln add tests/Apex.IntegrationTests/Apex.IntegrationTests.csproj
dotnet sln Apex.sln add tests/Apex.ArchitectureTests/Apex.ArchitectureTests.csproj

Write-Host "Projetos adicionados a solution" -ForegroundColor Green

# Limpar arquivos default
Write-Host "Limpando arquivos default..." -ForegroundColor Yellow
Remove-Item src/Apex.Domain/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/Apex.Application/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/Apex.Infrastructure/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/Apex.Shared/Class1.cs -ErrorAction SilentlyContinue
Remove-Item tests/Apex.UnitTests/UnitTest1.cs -ErrorAction SilentlyContinue
Remove-Item tests/Apex.IntegrationTests/UnitTest1.cs -ErrorAction SilentlyContinue
Remove-Item tests/Apex.ArchitectureTests/UnitTest1.cs -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "  Projeto Apex criado com sucesso!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Estrutura criada:" -ForegroundColor Yellow
Write-Host "  - Apex.sln" -ForegroundColor Gray
Write-Host "  - 5 projetos em src/" -ForegroundColor Gray
Write-Host "  - 3 projetos de teste em tests/" -ForegroundColor Gray
Write-Host ""
Write-Host "Proximos passos:" -ForegroundColor Yellow
Write-Host "  1. Adicionar referencias: dotnet add reference" -ForegroundColor Gray
Write-Host "  2. Adicionar pacotes: dotnet add package" -ForegroundColor Gray
Write-Host "  3. Compilar: dotnet build" -ForegroundColor Gray
Write-Host ""