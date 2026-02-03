USE salle_sport;

# Lister les membres qui ont réservé au moins un cours
SELECT nom, prenom
FROM Membre
WHERE id_membre IN (SELECT id_membre FROM reservation);

# Lister les cours dont la capacité est supérieure à la moyenne
SELECT nom_cours, capacite_max
FROM Cours
WHERE capacite_max > (SELECT AVG(capacite_max) FROM Cours);

# Lister les noms et prénoms des membres ET des coachs
SELECT nom, prenom FROM Membre
UNION
SELECT nom, prenom FROM Coach;

# Afficher les membres avec leur email
SELECT m.nom, m.prenom, u.email
FROM Membre m
JOIN Utilisateur u ON m.id_utilisateur = u.id_utilisateur;

# Afficher les réservations avec le nom du cours
SELECT m.nom, m.prenom, c.nom_cours, r.date_reservation
FROM Reservation r
JOIN Membre m ON r.id_membre = m.id_membre
JOIN Cours c ON r.id_cours = c.id_cours;

# Afficher tous les membres, même ceux qui n’ont jamais réservé
SELECT m.nom, m.prenom, r.id_reservation
FROM Membre m
LEFT JOIN Reservation r ON m.id_membre = r.id_membre;

# Afficher tous les cours, même ceux sans réservation
SELECT c.nom_cours, r.id_reservation
FROM Reservation r
RIGHT JOIN Cours c ON r.id_cours = c.id_cours;

# Nombre total de membres
SELECT COUNT(*) AS nombre_membres
FROM Membre;

# Capacité totale de tous les cours
SELECT SUM(capacite_max) AS capacite_totale
FROM Cours;

# Capacité moyenne des cours
SELECT AVG(capacite_max) AS capacite_moyenne
FROM Cours;

# Durée minimale d’un cours
SELECT MIN(duree_minutes) AS duree_min
FROM Cours;

# Durée maximale d’un cours
SELECT MAX(duree_minutes) AS duree_max
FROM Cours;

# Nombre de réservations par cours
SELECT c.nom_cours, COUNT(r.id_reservation) AS nb_reservations
FROM Cours c
LEFT JOIN Reservation r ON c.id_cours = r.id_cours
GROUP BY c.nom_cours
HAVING COUNT(r.id_reservation) >= 0;