using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;

namespace SampleAPI.Context;

public partial class PersonContext : DbContext
{
    /// <summary>
    /// This is a DB context created based on the Person schema of the AdventureWorks database.
    /// </summary>
    public PersonContext()
    {
    }

    /// <summary>
    /// This is a DB context created based on the Person schema of the AdventureWorks database.
    /// </summary>
    public PersonContext(DbContextOptions<PersonContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AddressType> AddressTypes { get; set; }

    public virtual DbSet<BusinessEntity> BusinessEntities { get; set; }

    public virtual DbSet<BusinessEntityAddress> BusinessEntityAddresses { get; set; }

    public virtual DbSet<BusinessEntityContact> BusinessEntityContacts { get; set; }

    public virtual DbSet<ContactType> ContactTypes { get; set; }

    public virtual DbSet<CountryRegion> CountryRegions { get; set; }

    public virtual DbSet<EmailAddress> EmailAddresses { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<PersonPhone> PersonPhones { get; set; }

    public virtual DbSet<PhoneNumberType> PhoneNumberTypes { get; set; }

    public virtual DbSet<StateProvince> StateProvinces { get; set; }

    public virtual DbSet<VAdditionalContactInfo> VAdditionalContactInfos { get; set; }

    public virtual DbSet<VStateProvinceCountryRegion> VStateProvinceCountryRegions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK_Address_AddressID");

            entity.ToTable("Address", "Person", tb => tb.HasComment("Street address information for customers, employees, and vendors."));

            entity.Property(e => e.AddressId).HasComment("Primary key for Address records.");
            entity.Property(e => e.AddressLine1).HasComment("First street address line.");
            entity.Property(e => e.AddressLine2).HasComment("Second street address line.");
            entity.Property(e => e.City).HasComment("Name of the city.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.PostalCode).HasComment("Postal code for the street address.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            entity.Property(e => e.StateProvinceId).HasComment("Unique identification number for the state or province. Foreign key to StateProvince table.");

            entity.HasOne(d => d.StateProvince).WithMany(p => p.Addresses).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AddressType>(entity =>
        {
            entity.HasKey(e => e.AddressTypeId).HasName("PK_AddressType_AddressTypeID");

            entity.ToTable("AddressType", "Person", tb => tb.HasComment("Types of addresses stored in the Address table. "));

            entity.Property(e => e.AddressTypeId).HasComment("Primary key for AddressType records.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Name).HasComment("Address type description. For example, Billing, Home, or Shipping.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
        });

        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityId).HasName("PK_BusinessEntity_BusinessEntityID");

            entity.ToTable("BusinessEntity", "Person", tb => tb.HasComment("Source of the ID that connects vendors, customers, and employees with address and contact information."));

            entity.Property(e => e.BusinessEntityId).HasComment("Primary key for all customers, vendors, and employees.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
        });

        modelBuilder.Entity<BusinessEntityAddress>(entity =>
        {
            entity.HasKey(e => new { e.BusinessEntityId, e.AddressId, e.AddressTypeId }).HasName("PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID");

            entity.ToTable("BusinessEntityAddress", "Person", tb => tb.HasComment("Cross-reference table mapping customers, vendors, and employees to their addresses."));

            entity.Property(e => e.BusinessEntityId).HasComment("Primary key. Foreign key to BusinessEntity.BusinessEntityID.");
            entity.Property(e => e.AddressId).HasComment("Primary key. Foreign key to Address.AddressID.");
            entity.Property(e => e.AddressTypeId).HasComment("Primary key. Foreign key to AddressType.AddressTypeID.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");

            entity.HasOne(d => d.Address).WithMany(p => p.BusinessEntityAddresses).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.AddressType).WithMany(p => p.BusinessEntityAddresses).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.BusinessEntity).WithMany(p => p.BusinessEntityAddresses).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BusinessEntityContact>(entity =>
        {
            entity.HasKey(e => new { e.BusinessEntityId, e.PersonId, e.ContactTypeId }).HasName("PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID");

            entity.ToTable("BusinessEntityContact", "Person", tb => tb.HasComment("Cross-reference table mapping stores, vendors, and employees to people"));

            entity.Property(e => e.BusinessEntityId).HasComment("Primary key. Foreign key to BusinessEntity.BusinessEntityID.");
            entity.Property(e => e.PersonId).HasComment("Primary key. Foreign key to Person.BusinessEntityID.");
            entity.Property(e => e.ContactTypeId).HasComment("Primary key.  Foreign key to ContactType.ContactTypeID.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");

            entity.HasOne(d => d.BusinessEntity).WithMany(p => p.BusinessEntityContacts).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ContactType).WithMany(p => p.BusinessEntityContacts).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Person).WithMany(p => p.BusinessEntityContacts).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ContactType>(entity =>
        {
            entity.HasKey(e => e.ContactTypeId).HasName("PK_ContactType_ContactTypeID");

            entity.ToTable("ContactType", "Person", tb => tb.HasComment("Lookup table containing the types of business entity contacts."));

            entity.Property(e => e.ContactTypeId).HasComment("Primary key for ContactType records.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Name).HasComment("Contact type description.");
        });

        modelBuilder.Entity<CountryRegion>(entity =>
        {
            entity.HasKey(e => e.CountryRegionCode).HasName("PK_CountryRegion_CountryRegionCode");

            entity.ToTable("CountryRegion", "Person", tb => tb.HasComment("Lookup table containing the ISO standard codes for countries and regions."));

            entity.Property(e => e.CountryRegionCode).HasComment("ISO standard code for countries and regions.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Name).HasComment("Country or region name.");
        });

        modelBuilder.Entity<EmailAddress>(entity =>
        {
            entity.HasKey(e => new { e.BusinessEntityId, e.EmailAddressId }).HasName("PK_EmailAddress_BusinessEntityID_EmailAddressID");

            entity.ToTable("EmailAddress", "Person", tb => tb.HasComment("Where to send a person email."));

            entity.Property(e => e.BusinessEntityId).HasComment("Primary key. Person associated with this email address.  Foreign key to Person.BusinessEntityID");
            entity.Property(e => e.EmailAddressId)
                .ValueGeneratedOnAdd()
                .HasComment("Primary key. ID of this email address.");
            entity.Property(e => e.EmailAddress1).HasComment("E-mail address for the person.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");

            entity.HasOne(d => d.BusinessEntity).WithMany(p => p.EmailAddresses).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityId).HasName("PK_Password_BusinessEntityID");

            entity.ToTable("Password", "Person", tb => tb.HasComment("One way hashed authentication information"));

            entity.Property(e => e.BusinessEntityId).ValueGeneratedNever();
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.PasswordHash).HasComment("Password for the e-mail account.");
            entity.Property(e => e.PasswordSalt).HasComment("Random value concatenated with the password string before the password is hashed.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");

            entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Password).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityId).HasName("PK_Person_BusinessEntityID");

            entity.ToTable("Person", "Person", tb =>
                {
                    tb.HasComment("Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.");
                    tb.HasTrigger("iuPerson");
                });

            entity.Property(e => e.BusinessEntityId)
                .ValueGeneratedNever()
                .HasComment("Primary key for Person records.");
            entity.Property(e => e.AdditionalContactInfo).HasComment("Additional contact information about the person stored in xml format. ");
            entity.Property(e => e.Demographics).HasComment("Personal information such as hobbies, and income collected from online shoppers. Used for sales analysis.");
            entity.Property(e => e.EmailPromotion).HasComment("0 = Contact does not wish to receive e-mail promotions, 1 = Contact does wish to receive e-mail promotions from AdventureWorks, 2 = Contact does wish to receive e-mail promotions from AdventureWorks and selected partners. ");
            entity.Property(e => e.FirstName).HasComment("First name of the person.");
            entity.Property(e => e.LastName).HasComment("Last name of the person.");
            entity.Property(e => e.MiddleName).HasComment("Middle name or middle initial of the person.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.NameStyle).HasComment("0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.");
            entity.Property(e => e.PersonType)
                .IsFixedLength()
                .HasComment("Primary type of person: SC = Store Contact, IN = Individual (retail) customer, SP = Sales person, EM = Employee (non-sales), VC = Vendor contact, GC = General contact");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            entity.Property(e => e.Suffix).HasComment("Surname suffix. For example, Sr. or Jr.");
            entity.Property(e => e.Title).HasComment("A courtesy title. For example, Mr. or Ms.");

            entity.HasOne(d => d.BusinessEntity).WithOne(p => p.Person).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PersonPhone>(entity =>
        {
            entity.HasKey(e => new { e.BusinessEntityId, e.PhoneNumber, e.PhoneNumberTypeId }).HasName("PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID");

            entity.ToTable("PersonPhone", "Person", tb => tb.HasComment("Telephone number and type of a person."));

            entity.Property(e => e.BusinessEntityId).HasComment("Business entity identification number. Foreign key to Person.BusinessEntityID.");
            entity.Property(e => e.PhoneNumber).HasComment("Telephone number identification number.");
            entity.Property(e => e.PhoneNumberTypeId).HasComment("Kind of phone number. Foreign key to PhoneNumberType.PhoneNumberTypeID.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");

            entity.HasOne(d => d.BusinessEntity).WithMany(p => p.PersonPhones).OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PhoneNumberType).WithMany(p => p.PersonPhones).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PhoneNumberType>(entity =>
        {
            entity.HasKey(e => e.PhoneNumberTypeId).HasName("PK_PhoneNumberType_PhoneNumberTypeID");

            entity.ToTable("PhoneNumberType", "Person", tb => tb.HasComment("Type of phone number of a person."));

            entity.Property(e => e.PhoneNumberTypeId).HasComment("Primary key for telephone number type records.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Name).HasComment("Name of the telephone number type");
        });

        modelBuilder.Entity<StateProvince>(entity =>
        {
            entity.HasKey(e => e.StateProvinceId).HasName("PK_StateProvince_StateProvinceID");

            entity.ToTable("StateProvince", "Person", tb => tb.HasComment("State and province lookup table."));

            entity.Property(e => e.StateProvinceId).HasComment("Primary key for StateProvince records.");
            entity.Property(e => e.CountryRegionCode).HasComment("ISO standard country or region code. Foreign key to CountryRegion.CountryRegionCode. ");
            entity.Property(e => e.IsOnlyStateProvinceFlag)
                .HasDefaultValue(true)
                .HasComment("0 = StateProvinceCode exists. 1 = StateProvinceCode unavailable, using CountryRegionCode.");
            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Date and time the record was last updated.");
            entity.Property(e => e.Name).HasComment("State or province description.");
            entity.Property(e => e.Rowguid)
                .HasDefaultValueSql("(newid())")
                .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            entity.Property(e => e.StateProvinceCode)
                .IsFixedLength()
                .HasComment("ISO standard state or province code.");
            entity.Property(e => e.TerritoryId).HasComment("ID of the territory in which the state or province is located. Foreign key to SalesTerritory.SalesTerritoryID.");

            entity.HasOne(d => d.CountryRegionCodeNavigation).WithMany(p => p.StateProvinces).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VAdditionalContactInfo>(entity =>
        {
            entity.ToView("vAdditionalContactInfo", "Person");
        });

        modelBuilder.Entity<VStateProvinceCountryRegion>(entity =>
        {
            entity.ToView("vStateProvinceCountryRegion", "Person");

            entity.Property(e => e.StateProvinceCode).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
