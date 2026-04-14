SET NAMES 'utf8mb4';
SET CHARACTER SET utf8mb4;

CREATE DATABASE IF NOT EXISTS library_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE library_db;

-- Drop in reverse dependency order so FK constraints don't block the drops
DROP TABLE IF EXISTS loans;
DROP TABLE IF EXISTS books;
DROP TABLE IF EXISTS users;

CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARBINARY(128) NOT NULL,
    salt VARBINARY(64) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE books (
    id INT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(255) NOT NULL,
    author VARCHAR(150) NOT NULL,
    genre VARCHAR(100),
    isbn VARCHAR(20),
    publication_year INT,
    rayon VARCHAR(100) NOT NULL,
    etagere VARCHAR(100) NOT NULL,
    is_available BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE loans (
    id INT PRIMARY KEY AUTO_INCREMENT,
    book_id INT NOT NULL,
    user_id INT NOT NULL,
    borrow_date DATETIME NOT NULL,
    due_date DATETIME NOT NULL,
    return_date DATETIME NULL,
    CONSTRAINT fk_book FOREIGN KEY (book_id) REFERENCES books(id) ON DELETE CASCADE,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX idx_book_title ON books(title);
CREATE INDEX idx_user_username ON users(username);

INSERT INTO books (title, author, genre, isbn, publication_year, rayon, etagere, is_available) VALUES
('Le Seigneur des Anneaux', 'J.R.R. Tolkien', 'Fantasy', '9782266154116', 1954, 'Salon', 'Étagère A1', 1),
('1984', 'George Orwell', 'Dystopie', '9780451524935', 1949, 'Bureau', 'Meuble Haut', 1),
('Le Petit Prince', 'Antoine de Saint-Exupéry', 'Conte', '9782070408504', 1943, 'Chambre', 'Table de chevet', 1),
('Dune', 'Frank Herbert', 'Science-Fiction', '9782221002872', 1965, 'Salon', 'Étagère B2', 1),
('Fondation', 'Isaac Asimov', 'Science-Fiction', '9782070415700', 1951, 'Salon', 'Étagère B2', 1),
('L''Étranger', 'Albert Camus', 'Roman', '9782070360024', 1942, 'Bureau', 'Étagère Classiques', 1),
('Harry Potter à l''école des sorciers', 'J.K. Rowling', 'Fantasy', '9782070518425', 1997, 'Chambre', 'Bibliothèque', 1),
('Le Neuromancien', 'William Gibson', 'Cyberpunk', '9782290006191', 1984, 'Salon', 'Étagère SF', 1),
('Les Misérables', 'Victor Hugo', 'Roman', '9782070408993', 1862, 'Bureau', 'Étagère Classiques', 1),
('Le Cycle de Cyann', 'François Bourgeon', 'Bande Dessinée', '9782203388574', 1993, 'Salon', 'Rayon BD', 1),
('Chroniques Martiennes', 'Ray Bradbury', 'Science-Fiction', '9782207249413', 1950, 'Salon', 'Étagère B2', 1),
('La Nuit des temps', 'René Barjavel', 'Science-Fiction', '9782266027310', 1968, 'Chambre', 'Étagère Sud', 1),
('Le Nom de la Rose', 'Umberto Eco', 'Policier', '9782253033134', 1980, 'Bureau', 'Meuble Haut', 1),
('Fahrenheit 451', 'Ray Bradbury', 'Dystopie', '9782070415342', 1953, 'Salon', 'Étagère B2', 1),
('Ubik', 'Philip K. Dick', 'Science-Fiction', '9782264040343', 1969, 'Salon', 'Étagère SF', 1);
