# gimzo database

## Setup

### Users

The first script to run is `1_create_users.sql` - this file creates the two users used by gimzo.
One user is a read-only user, and the other is for executing commands.

In your PostgreSQL instance, run the following script from the command line.
The users (roles) are created across your instance, so you only have to run this once.

```psql
> \i 1_create_users.sql
```

### Databases

I like to set up three databases, one for test, one for dev, and one for production.
These correspond to the connection strings for the infrastructure tests, my local dev environment, and my "production" environment.
My production environment is created using the deploy script in the `/eng` directory.

From a PSQL prompt, I run:

```psql
\i 2_create_admin_tables.sql
\i 3_create_import_tables.sql
\i 4_create_runtime_tables.sql
\i 6_grant_permissions.sql
```

I run these three SQL scripts for each database; here is the full set of commands:

```psql
CREATE DATABASE gimzo_test;
CREATE DATABASE gimzo_dev;
CREATE DATABASE gimzo_prod;
\c gimzo_test
\i 2_create_admin_tables.sql
\i 3_create_import_tables.sql
\i 4_create_runtime_tables.sql
\i 6_grant_permissions.sql
\c gimzo_dev
\i 2_create_admin_tables.sql
\i 3_create_import_tables.sql
\i 4_create_runtime_tables.sql
\i 6_grant_permissions.sql
\c gimzo_prod
\i 2_create_admin_tables.sql
\i 3_create_import_tables.sql
\i 4_create_runtime_tables.sql
\i 6_grant_permissions.sql
```

## Other notes

### Making a copy of the database.
The following sequence of commands will migrate your dev database to prod (on Windows).

```
pg_dump -Fc -v -U postgres -h localhost gimzo_dev > gimzo_dev.dump
dropdb -U postgres -h localhost gimzo_prod
createdb -U postgres -h localhost gimzo_prod
pg_restore -v -j 8 --clean --if-exists -U postgres -h localhost -d gimzo_prod gimzo_dev.dump
```