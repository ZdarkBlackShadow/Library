SET NAMES 'utf8mb4';
SET CHARACTER SET utf8mb4;


CREATE DATABASE IF NOT EXISTS library_db 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE library_db;

CREATE TABLE IF NOT EXISTS users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARBINARY(128) NOT NULL,
    salt VARBINARY(64) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS books (
    id INT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(255) NOT NULL,
    author VARCHAR(150) NOT NULL,
    isbn VARCHAR(20),
    location_description VARCHAR(255) NOT NULL,
    is_available BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS loans (
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

INSERT INTO books (title, author, isbn, location_description, is_available) VALUES
('Le Seigneur des Anneaux', 'J.R.R. Tolkien', '9782266154116', 'Salon, Étagère A1', 1),
('1984', 'George Orwell', '9780451524935', 'Bureau, Meuble Haut', 1),
('Le Petit Prince', 'Antoine de Saint-Exupéry', '9782070408504', 'Chambre, Table de chevet', 1),
('Dune', 'Frank Herbert', '9782221002872', 'Salon, Étagère B2', 1),
('Fondation', 'Isaac Asimov', '9782070415700', 'Salon, Étagère B2', 1),
('L''Étranger', 'Albert Camus', '9782070360024', 'Bureau, Étagère Classiques', 1),
('Harry Potter à l''école des sorciers', 'J.K. Rowling', '9782070518425', 'Chambre, Bibliothèque', 1),
('Le Neuromancien', 'William Gibson', '9782290006191', 'Salon, Étagère SF', 1),
('Les Misérables', 'Victor Hugo', '9782070408993', 'Bureau, Étagère Classiques', 1),
('Le Cycle de Cyann', 'François Bourgeon', '9782203388574', 'Salon, Rayon BD', 1),
('Chroniques Martiennes', 'Ray Bradbury', '9782207249413', 'Salon, Étagère B2', 1),
('La Nuit des temps', 'René Barjavel', '9782266027310', 'Chambre, Étagère Sud', 1),
('Le Nom de la Rose', 'Umberto Eco', '9782253033134', 'Bureau, Meuble Haut', 1),
('Fahrenheit 451', 'Ray Bradbury', '9782070415342', 'Salon, Étagère B2', 1),
('Ubik', 'Philip K. Dick', '9782264040343', 'Salon, Étagère SF', 1);
