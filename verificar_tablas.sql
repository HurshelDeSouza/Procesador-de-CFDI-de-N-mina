-- Script para verificar si las tablas de nómina existen
USE DescargaCfdiGFP;
GO

-- Verificar tablas existentes
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Nomina%'
ORDER BY TABLE_NAME;

-- Si no existen, ejecutar el script facturas.sql que las crea
