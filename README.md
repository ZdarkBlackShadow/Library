# Rat de Bibliothèque

Application de gestion de bibliothèque en C# / WinForms (.NET 10), avec base de données MySQL.

---

## Prérequis

| Outil | Version minimale |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [Docker](https://www.docker.com/) | 24+ |
| [Wine](https://www.winehq.org/) *(Linux uniquement)* | 9.0+ |

---

## 1. Démarrer la base de données

```bash
docker-compose up -d
```

Le conteneur MySQL démarre sur le port **3306** et exécute automatiquement
`scripts/database.sql` au premier lancement (tables + données de test).

Pour réinitialiser complètement la base (supprime toutes les données) :

```bash
docker-compose down -v
docker-compose up -d
```

---

## 2. Compiler

```bash
dotnet build
```

### Publier un exécutable autonome (self-contained)

```bash
dotnet publish Library.UI/Library.UI.csproj -c Release -r win-x64 --self-contained true -o publish/
```

---

## 3. Lancer l'application

### Linux (via Wine)

```bash
# Après un build Debug :
wine Library.UI/bin/Debug/net10.0-windows/win-x64/Library.UI.exe

# Après un publish dans publish/ :
wine publish/Library.UI.exe
```

> Les messages `fixme:` dans la console Wine sont normaux et n'affectent pas le fonctionnement.

### Windows

```cmd
# Après un build Debug :
Library.UI\bin\Debug\net10.0-windows\win-x64\Library.UI.exe

# Après un publish dans publish\ :
publish\Library.UI.exe
```

---

## Structure du projet

```
Library/
├── docker-compose.yml
├── scripts/
│   └── database.sql              # Schéma + données initiales
├── Library.Core/                 # Logique métier (Models, Services, DB)
│   ├── Models/
│   │   ├── Book.cs
│   │   ├── Loan.cs
│   │   └── User.cs
│   ├── Services/
│   │   ├── BookService.cs
│   │   ├── LoanService.cs
│   │   └── UserService.cs
│   └── Data/
│       └── DatabaseContext.cs
└── Library.UI/                   # Interface WinForms
    ├── Program.cs
    ├── Form1.cs                  # Écran de connexion
    ├── MainForm.cs               # Fenêtre principale
    ├── Forms/
    │   └── BookEditForm.cs       # Ajout / modification de livre
    └── Helpers/
        └── UIHelper.cs           # Système de design
```

---

## Champs d'un livre

| Champ | Type | Obligatoire |
|---|---|---|
| Titre | Texte | Oui |
| Auteur | Texte | Oui |
| Genre | Texte | Non |
| ISBN | Texte | Non |
| Année de publication | Entier (1000–2100) | Non |
| Rayon | Texte | Oui |
| Étagère | Texte | Oui |
| Disponible | Booléen | — (automatique) |

---

## Connexion par défaut

Au premier lancement, créez un compte via le bouton **"Créer un compte"**.
Les mots de passe sont hachés avec **Argon2id** (64 Mo, 4 itérations).
