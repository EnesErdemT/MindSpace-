using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=MediumCloneDB;Username=postgres;Password=tERDEM4006";

using var connection = new NpgsqlConnection(connectionString);
connection.Open();

Console.WriteLine("🔄 Updating database to Romanian...");

var updates = new[]
{
    // Categories
    ("UPDATE \"Categories\" SET \"Name\" = 'Tehnologie', \"Slug\" = 'tehnologie' WHERE \"Name\" = 'Teknoloji'", "Tehnologie"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Inteligență Artificială', \"Slug\" = 'inteligenta-artificiala' WHERE \"Name\" = 'Yapay Zeka'", "Inteligență Artificială"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Tehnologie 5G', \"Slug\" = 'tehnologie-5g' WHERE \"Name\" = '5G Teknolojisi'", "Tehnologie 5G"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Software', \"Slug\" = 'dezvoltare-software' WHERE \"Name\" = 'Yazılım Geliştirme'", "Dezvoltare Software"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Cod Curat', \"Slug\" = 'cod-curat' WHERE \"Name\" = 'Clean Code'", "Cod Curat"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Principii SOLID', \"Slug\" = 'principii-solid' WHERE \"Name\" = 'SOLID Prensipleri'", "Principii SOLID"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Web', \"Slug\" = 'dezvoltare-web' WHERE \"Name\" = 'Web Development'", "Dezvoltare Web"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Frontend', \"Slug\" = 'dezvoltare-frontend' WHERE \"Name\" = 'Frontend Development'", "Dezvoltare Frontend"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Backend', \"Slug\" = 'dezvoltare-backend' WHERE \"Name\" = 'Backend Development'", "Dezvoltare Backend"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Full Stack', \"Slug\" = 'dezvoltare-full-stack' WHERE \"Name\" = 'Full Stack Development'", "Dezvoltare Full Stack"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Mobile', \"Slug\" = 'dezvoltare-mobile' WHERE \"Name\" = 'Mobile Development'", "Dezvoltare Mobile"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare iOS', \"Slug\" = 'dezvoltare-ios' WHERE \"Name\" = 'iOS Development'", "Dezvoltare iOS"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Dezvoltare Android', \"Slug\" = 'dezvoltare-android' WHERE \"Name\" = 'Android Development'", "Dezvoltare Android"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Baze de Date', \"Slug\" = 'baze-de-date' WHERE \"Name\" = 'Database'", "Baze de Date"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Securitate', \"Slug\" = 'securitate' WHERE \"Name\" = 'Security'", "Securitate"),
    ("UPDATE \"Categories\" SET \"Name\" = 'Carieră', \"Slug\" = 'cariera' WHERE \"Name\" = 'Career'", "Carieră"),
};

int totalUpdated = 0;
foreach (var (sql, name) in updates)
{
    using var cmd = new NpgsqlCommand(sql, connection);
    var affected = cmd.ExecuteNonQuery();
    if (affected > 0)
    {
        Console.WriteLine($"✅ Updated: {name} ({affected} rows)");
        totalUpdated += affected;
    }
}

Console.WriteLine($"\n✅ Total updated: {totalUpdated} categories");
Console.WriteLine("✅ Database updated successfully!");
