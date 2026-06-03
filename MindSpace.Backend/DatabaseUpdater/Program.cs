using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=MediumCloneDB;Username=postgres;Password=tERDEM4006";

using var connection = new NpgsqlConnection(connectionString);
connection.Open();

Console.WriteLine("🗑️ Ștergere date existente...\n");

// Delete in correct order due to foreign key constraints
Console.WriteLine("Ștergere Notifications...");
using var deleteNotificationsCmd = new NpgsqlCommand("DELETE FROM \"Notifications\"", connection);
int notificationsDeleted = deleteNotificationsCmd.ExecuteNonQuery();
Console.WriteLine($"✅ {notificationsDeleted} notificări șterse\n");

Console.WriteLine("Ștergere Likes...");
using var deleteLikesCmd = new NpgsqlCommand("DELETE FROM \"Likes\"", connection);
int likesDeleted = deleteLikesCmd.ExecuteNonQuery();
Console.WriteLine($"✅ {likesDeleted} likes șterse\n");

Console.WriteLine("Ștergere Comments...");
using var deleteCommentsCmd = new NpgsqlCommand("DELETE FROM \"Comments\"", connection);
int commentsDeleted = deleteCommentsCmd.ExecuteNonQuery();
Console.WriteLine($"✅ {commentsDeleted} comentarii șterse\n");

Console.WriteLine("Ștergere Bookmarks...");
using var deleteBookmarksCmd = new NpgsqlCommand("DELETE FROM \"Bookmarks\"", connection);
int bookmarksDeleted = deleteBookmarksCmd.ExecuteNonQuery();
Console.WriteLine($"✅ {bookmarksDeleted} bookmarks șterse\n");

Console.WriteLine("Ștergere PostTags...");
using var deletePostTagsCmd = new NpgsqlCommand("DELETE FROM \"PostTags\"", connection);
int postTagsDeleted = deletePostTagsCmd.ExecuteNonQuery();
Console.WriteLine($"✅ {postTagsDeleted} post-tag relații șterse\n");

Console.WriteLine("Ștergere Posts...");
using var deletePostsCmd = new NpgsqlCommand("DELETE FROM \"Posts\"", connection);
int postsDeleted = deletePostsCmd.ExecuteNonQuery();
Console.WriteLine($"✅ {postsDeleted} posturi șterse\n");

Console.WriteLine("📊 Verificare finală:");
using var verifyCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Posts\"", connection);
var count = verifyCmd.ExecuteScalar();
Console.WriteLine($"Total posturi rămase în baza de date: {count}");

Console.WriteLine("\n✅ Baza de date este pregătită pentru re-seeding!");
Console.WriteLine("💡 Acum repornește backend-ul pentru a re-seed datele în română.");
