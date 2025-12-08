using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace PizzaMizza;

internal static class Program {
	private static readonly string ConnectionString =
		"Server=localhost,1433;Database=PizzaMizza;User Id=sa;Password=@FooBar();Encrypt=True;TrustServerCertificate=True;\n";

	static void Main() {
		Console.OutputEncoding = Encoding.UTF8;

		while (true) {
			Console.WriteLine("1 - Bütün pizzaları göstər");
			Console.WriteLine("2 - Yeni pizza əlavə et");
			Console.WriteLine("0 - Çıxış");
			var choice = Console.ReadLine();

			switch (choice) {
				case "1":
					ShowPizzas();
					break;
				case "2":
					AddPizza();
					break;
				case "0":
					return;
			}
		}
	}

	// TODO: ------------------------- HELPER METHODS -------------------------

	static SqlConnection GetConnection() => new(ConnectionString);

	static SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters) {
		var conn = GetConnection();
		conn.Open();
		var cmd = new SqlCommand(sql, conn);
		cmd.Parameters.AddRange(parameters);
		return cmd.ExecuteReader(CommandBehavior.CloseConnection);
	}

	static object? ExecuteScalar(string sql, params SqlParameter[] parameters) {
		using var conn = GetConnection();
		conn.Open();
		using var cmd = new SqlCommand(sql, conn);
		cmd.Parameters.AddRange(parameters);
		return cmd.ExecuteScalar();
	}

	static void ExecuteNonQuery(string sql, params SqlParameter[] parameters) {
		using var conn = GetConnection();
		conn.Open();
		using var cmd = new SqlCommand(sql, conn);
		cmd.Parameters.AddRange(parameters);
		cmd.ExecuteNonQuery();
	}

	static int ReadInt(string label) {
		Console.Write(label);
		int.TryParse(Console.ReadLine(), out int value);
		return value;
	}

	static string? ReadString(string label) {
		Console.Write(label);
		return Console.ReadLine();
	}

	// TODO: ------------------------- FEATURES -------------------------

	static void ShowPizzas() {
		const string sql = "SELECT Id, Name, Type FROM Pizza";

		using (var reader = ExecuteReader(sql)) {
			Console.WriteLine("Pizzalar:");
			while (reader.Read()) {
				Console.WriteLine($"{reader["Id"]} - {reader["Name"]} ({reader["Type"]})");
			}
		}

		int id = ReadInt("Ətraflı məlumat üçün pizza Id (0 = geri): ");
		if (id != 0)
			ShowPizzaDetails(id);
	}

	static void ShowPizzaDetails(int pizzaId) {
		Console.WriteLine("Ingredients:");
		string ingSql =
			@"SELECT i.Name 
              FROM Ingredient i
              JOIN PizzaIngredient pi ON pi.IngredientId = i.Id
              WHERE pi.PizzaId = @id";

		using (var r = ExecuteReader(ingSql, new SqlParameter("@id", pizzaId)))
			while (r.Read())
				Console.WriteLine($"- {r["Name"]}");

		Console.WriteLine("Qiymətlər:");
		string priceSql =
			@"SELECT s.Name AS Size, pp.Price
              FROM PizzaPrice pp
              JOIN Size s ON s.Id = pp.SizeId
              WHERE pp.PizzaId = @id";

		using (var r = ExecuteReader(priceSql, new SqlParameter("@id", pizzaId)))
			while (r.Read())
				Console.WriteLine($"{r["Size"]}: {r["Price"]} AZN");
	}

	static void AddPizza() {
		string? name = ReadString("Pizza adı: ");
		string? type = ReadString("Pizza tipi: ");

		int pizzaId = (int)(ExecuteScalar(
			"INSERT INTO Pizza(Name, Type) OUTPUT INSERTED.Id VALUES(@n, @t)",
			new SqlParameter("@n", name),
			new SqlParameter("@t", type)
		) ?? 0);

		AddIngredients(pizzaId);
		AddPrices(pizzaId);

		Console.WriteLine("Yeni pizza əlavə edildi!");
	}

	static void AddIngredients(int pizzaId) {
		Console.WriteLine("Mövcud ingredientlər:");
		var ingredients = new Dictionary<int, string?>();

		using (var r = ExecuteReader("SELECT Id, Name FROM Ingredient")) {
			while (r.Read()) {
				int id = (int)r["Id"];
				ingredients[id] = r["Name"].ToString();
				Console.WriteLine($"{id} - {ingredients[id]}");
			}
		}

		Console.Write("Ingredientləri seçin (1,3,5): ");
		var selected = Console.ReadLine()?.Split(',');

		if (selected != null)
			foreach (var s in selected) {
				int ingId = int.Parse(s.Trim());
				ExecuteNonQuery(
					"INSERT INTO PizzaIngredient(PizzaId, IngredientId) VALUES(@p, @i)",
					new SqlParameter("@p", pizzaId),
					new SqlParameter("@i", ingId)
				);
			}
	}

	static void AddPrices(int pizzaId) {
		Console.WriteLine("Mövcud ölçülər:");
		var sizes = new Dictionary<int, string?>();

		using (var r = ExecuteReader("SELECT Id, Name FROM Size")) {
			while (r.Read()) {
				int id = (int)r["Id"];
				sizes[id] = r["Name"].ToString();
				Console.WriteLine($"{id} - {sizes[id]}");
			}
		}

		Console.WriteLine("Hər ölçü üçün qiymət:");
		foreach (var s in sizes) {
			decimal price = ReadInt($"{s.Value} üçün qiymət: ");
			ExecuteNonQuery(
				"INSERT INTO PizzaPrice(PizzaId, SizeId, Price) VALUES(@p, @s, @pr)",
				new SqlParameter("@p", pizzaId),
				new SqlParameter("@s", s.Key),
				new SqlParameter("@pr", price)
			);
		}
	}
}
