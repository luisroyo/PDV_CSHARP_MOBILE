-- Script de configuração do banco de dados PostgreSQL para PDV Multi-Vertical
-- Execute este script como superuser (postgres) após criar o banco

-- Criar banco de dados
CREATE DATABASE pos_multivertical
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'pt_BR.UTF-8'
    LC_CTYPE = 'pt_BR.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Conectar ao banco
\c pos_multivertical;

-- Criar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Criar schema para multi-tenancy
CREATE SCHEMA IF NOT EXISTS tenants;

-- Configurar permissões
GRANT ALL PRIVILEGES ON DATABASE pos_multivertical TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA tenants TO postgres;

-- Criar índices para performance
-- (Os índices serão criados automaticamente pelo EF Core, mas podemos adicionar alguns específicos)

-- Índice para busca de produtos por código de barras
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_products_barcode ON "Products" ("Barcode");

-- Índice para busca de produtos por nome (usando pg_trgm)
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_products_name_trgm ON "Products" USING gin ("Name" gin_trgm_ops);

-- Índice para pedidos por data
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_created_at ON "Orders" ("CreatedAt");

-- Índice para itens de pedido por produto
-- CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_order_items_product_id ON "OrderItems" ("ProductId");

-- Configurar estatísticas
ALTER DATABASE pos_multivertical SET default_statistics_target = 100;

-- Comentários para documentação
COMMENT ON DATABASE pos_multivertical IS 'Banco de dados do sistema PDV Multi-Vertical';
COMMENT ON SCHEMA tenants IS 'Schema para dados multi-tenant';
