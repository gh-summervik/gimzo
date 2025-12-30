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

From a PSQL prompt, I run:

```psql
> \i 2_create_domain_tables.sql
> \i 3_create_admin_tables.sql
> \i 6_grant_permissions.sql
```

I run these three SQL scripts for each database; here is the full set of commands:

```psql
> CREATE DATABASE gimzo_test;
> CREATE DATABASE gimzo_dev;
> CREATE DATABASE gimzo_prod;
> \c gimzo_test
> \i 2_create_domain_tables.sql
> \i 3_create_admin_tables.sql
> \i 6_grant_permissions.sql
> \c gimzo_dev
> \i 2_create_domain_tables.sql
> \i 3_create_admin_tables.sql
> \i 6_grant_permissions.sql
> \c gimzo_prod
> \i 2_create_domain_tables.sql
> \i 3_create_admin_tables.sql
> \i 6_grant_permissions.sql
```
