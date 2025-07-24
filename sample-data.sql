-- Sample data for the SQL MCP Server
-- This script creates the actor table and populates it with sample data

-- Create the actor table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='actor' AND xtype='U')
BEGIN
    CREATE TABLE actor (
        actor_id INT IDENTITY(1,1) PRIMARY KEY,
        first_name NVARCHAR(45) NOT NULL,
        last_name NVARCHAR(45) NOT NULL,
        last_update DATETIME NOT NULL DEFAULT GETDATE()
    );
END

-- Insert sample actor data
INSERT INTO actor (first_name, last_name) VALUES
('John', 'Doe'),
('Jane', 'Smith'),
('Robert', 'Johnson'),
('Emily', 'Davis'),
('Michael', 'Wilson'),
('Sarah', 'Brown'),
('David', 'Jones'),
('Lisa', 'Garcia'),
('Christopher', 'Miller'),
('Amanda', 'Anderson'),
('Matthew', 'Taylor'),
('Jessica', 'Thomas'),
('James', 'Jackson'),
('Ashley', 'White'),
('Joshua', 'Harris'),
('Brittany', 'Martin'),
('Andrew', 'Thompson'),
('Samantha', 'Garcia'),
('Daniel', 'Martinez'),
('Nicole', 'Robinson'),
('Anthony', 'Clark'),
('Michelle', 'Rodriguez'),
('Mark', 'Lewis'),
('Stephanie', 'Lee'),
('Steven', 'Walker');

-- Display the inserted data
SELECT TOP 10 * FROM actor ORDER BY actor_id;
