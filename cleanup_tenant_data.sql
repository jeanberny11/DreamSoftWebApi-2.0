-- =============================================================================
-- DreamSoft — Tenant Data Cleanup Script
-- Purpose : Wipe all tenant-generated data for testing purposes.
--           Lookup/seed tables (statuses, plans, solutions, countries, etc.)
--           are intentionally preserved.
-- Usage   : Run in psql or any PostgreSQL client against dreamsoft_database.
--           Filter by a specific tenant using the DO block at the bottom,
--           or run as-is to wipe ALL tenants.
-- =============================================================================

BEGIN;

-- -----------------------------------------------------------------------------
-- 1. DEEPEST CHILDREN FIRST (no FK dependencies below these)
-- -----------------------------------------------------------------------------

-- Payment records (FK → subscription_invoices, tenants)
DELETE FROM subscription_payments
WHERE tenant_id IN (SELECT id FROM tenants);

-- User refresh tokens (FK → users  [CASCADE, but explicit is safer])
DELETE FROM user_refresh_tokens
WHERE user_id IN (SELECT id FROM users);

-- Role permissions assigned to tenant roles
DELETE FROM role_option_actions
WHERE role_id IN (SELECT id FROM roles);

DELETE FROM role_menu_options
WHERE role_id IN (SELECT id FROM roles);

-- -----------------------------------------------------------------------------
-- 2. CHILD TABLES THAT DEPEND ON users / roles / tenant_subscriptions
-- -----------------------------------------------------------------------------

-- Users (FK → roles [SetNull], languages, genders)
DELETE FROM users
WHERE tenant_id IN (SELECT id FROM tenants);

-- Roles (FK → tenants via tenant_id column, role_templates [SetNull])
DELETE FROM roles
WHERE tenant_id IN (SELECT id FROM tenants);

-- Subscription invoices (FK → tenant_subscriptions, tenants)
DELETE FROM subscription_invoices
WHERE tenant_id IN (SELECT id FROM tenants);

-- -----------------------------------------------------------------------------
-- 3. DIRECT TENANT CHILDREN
-- -----------------------------------------------------------------------------

-- Tenant subscriptions (FK → tenants, plan_prices, solutions, subscription_statuses)
DELETE FROM tenant_subscriptions
WHERE tenant_id IN (SELECT id FROM tenants);

-- Tenant subdomains
DELETE FROM tenant_subdomains
WHERE tenant_id IN (SELECT id FROM tenants);

-- Tenant refresh tokens
DELETE FROM tenant_refresh_tokens
WHERE tenant_id IN (SELECT id FROM tenants);

-- Tenant registration / email verification tokens
DELETE FROM tenant_registration_tokens
WHERE tenant_id IN (SELECT id FROM tenants);

-- -----------------------------------------------------------------------------
-- 4. TENANTS THEMSELVES
-- -----------------------------------------------------------------------------

DELETE FROM tenants;

-- -----------------------------------------------------------------------------
-- 5. RESET SEQUENCES (so IDs restart from 1 on next test run)
-- -----------------------------------------------------------------------------

ALTER SEQUENCE tenants_id_seq                  RESTART WITH 1;
ALTER SEQUENCE tenant_refresh_tokens_id_seq    RESTART WITH 1;
ALTER SEQUENCE tenant_registration_tokens_id_seq RESTART WITH 1;
ALTER SEQUENCE tenant_subdomains_id_seq        RESTART WITH 1;
ALTER SEQUENCE tenant_subscriptions_id_seq     RESTART WITH 1;
ALTER SEQUENCE subscription_invoices_id_seq    RESTART WITH 1;
ALTER SEQUENCE subscription_payments_id_seq    RESTART WITH 1;
ALTER SEQUENCE roles_id_seq                    RESTART WITH 1;
ALTER SEQUENCE users_id_seq                    RESTART WITH 1;
ALTER SEQUENCE user_refresh_tokens_id_seq      RESTART WITH 1;

COMMIT;

-- =============================================================================
-- WHAT IS PRESERVED (seed / lookup data — untouched by this script)
-- =============================================================================
--  billing_cycles            tenant_statuses
--  countries                 subscription_statuses
--  provinces                 subscription_plans
--  municipalities            plan_prices
--  languages                 plan_menu_options
--  currencies                plan_limits
--  genders                   role_templates
--  id_types                  role_menu_options_template
--  solutions                 role_option_actions_template
--  modules                   option_actions
--  menu_groups               menu_options
-- =============================================================================


-- =============================================================================
-- OPTIONAL: Delete a SINGLE tenant by email (comment out the block above and
-- uncomment this block instead when you want to target one specific tenant)
-- =============================================================================

-- DO $$
-- DECLARE
--     v_tenant_id INT;
-- BEGIN
--     SELECT id INTO v_tenant_id FROM tenants WHERE email = 'test@example.com';
--
--     IF v_tenant_id IS NULL THEN
--         RAISE NOTICE 'Tenant not found.';
--         RETURN;
--     END IF;
--
--     DELETE FROM subscription_payments      WHERE tenant_id = v_tenant_id;
--     DELETE FROM user_refresh_tokens        WHERE user_id IN (SELECT id FROM users WHERE tenant_id = v_tenant_id);
--     DELETE FROM role_option_actions        WHERE role_id  IN (SELECT id FROM roles WHERE tenant_id = v_tenant_id);
--     DELETE FROM role_menu_options          WHERE role_id  IN (SELECT id FROM roles WHERE tenant_id = v_tenant_id);
--     DELETE FROM users                      WHERE tenant_id = v_tenant_id;
--     DELETE FROM roles                      WHERE tenant_id = v_tenant_id;
--     DELETE FROM subscription_invoices      WHERE tenant_id = v_tenant_id;
--     DELETE FROM tenant_subscriptions       WHERE tenant_id = v_tenant_id;
--     DELETE FROM tenant_subdomains          WHERE tenant_id = v_tenant_id;
--     DELETE FROM tenant_refresh_tokens      WHERE tenant_id = v_tenant_id;
--     DELETE FROM tenant_registration_tokens WHERE tenant_id = v_tenant_id;
--     DELETE FROM tenants                    WHERE id        = v_tenant_id;
--
--     RAISE NOTICE 'Tenant % deleted successfully.', v_tenant_id;
-- END $$;
