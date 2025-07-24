-- SQL MCP Server - First Actor Query Examples
-- These are the SQL queries that the MCP server will execute when you ask for the first actor

-- Query 1: Get the first actor (used by GetActors tool with limit=1)
SELECT TOP 1 * 
FROM actor 
ORDER BY actor_id;

-- Query 2: Get actor by ID = 1 (used by GetActorById tool)
SELECT * 
FROM actor 
WHERE actor_id = 1;

-- Query 3: Get first actor with complete information
SELECT 
    actor_id,
    first_name,
    last_name,
    last_update
FROM actor 
WHERE actor_id = (SELECT MIN(actor_id) FROM actor);

-- Query 4: Check if actor table exists and has data
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'actor')
BEGIN
    SELECT 
        'Table exists' AS status,
        COUNT(*) AS total_actors,
        MIN(actor_id) AS first_actor_id,
        MAX(actor_id) AS last_actor_id
    FROM actor;
    
    -- Show the first actor
    SELECT TOP 1 
        'First Actor:' AS info,
        actor_id,
        first_name + ' ' + last_name AS full_name,
        last_update
    FROM actor 
    ORDER BY actor_id;
END
ELSE
BEGIN
    SELECT 'Actor table does not exist' AS status;
END

-- Query 5: Sample data check (run this first if you need to add data)
/*
-- Uncomment and run this section if your actor table is empty:

INSERT INTO actor (first_name, last_name) VALUES
('John', 'Doe'),
('Jane', 'Smith'),
('Robert', 'Johnson'),
('Emily', 'Davis'),
('Michael', 'Wilson');

SELECT 'Sample data inserted' AS status;
*/
