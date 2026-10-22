using Godot;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

public partial class Database : Node
{
	public static Database Instance { get; private set; }

	private SqliteConnection _connection;
	public SqliteConnection Connection => _connection;

	public bool IsOpen => _connection != null && _connection.State == System.Data.ConnectionState.Open;

	public override void _Ready()
	{
		Instance = this;
	}

	/// <summary>
	/// Abre ou cria um banco de dados SQLite para o save especificado (ex: "user://saves/save_01.db").
	/// </summary>
	public void OpenSaveDatabase(string godotPath)
	{
		CloseDatabase();

		string absolutePath = ProjectSettings.GlobalizePath(godotPath);
		string directory = Path.GetDirectoryName(absolutePath);

		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		string connectionString = new SqliteConnectionStringBuilder
		{
			DataSource = absolutePath,
			Mode = SqliteOpenMode.ReadWriteCreate,
			ForeignKeys = true
		}.ToString();

		_connection = new SqliteConnection(connectionString);
		_connection.Open();

		GD.Print($"[Database] Conectado com sucesso ao SQLite em: {absolutePath}");
	}

	/// <summary>
	/// Executa um comando SQL sem retorno (CREATE TABLE, INSERT, UPDATE, DELETE).
	/// </summary>
	public int ExecuteNonQuery(string sql)
	{
		if (!IsOpen)
		{
			GD.PushError("[Database] Tentativa de executar comando SQL com o banco fechado.");
			return 0;
		}

		using var command = _connection.CreateCommand();
		command.CommandText = sql;
		return command.ExecuteNonQuery();
	}

	public void CloseDatabase()
	{
		if (_connection != null)
		{
			_connection.Close();
			_connection.Dispose();
			_connection = null;
		}
	}

	public override void _ExitTree()
	{
		CloseDatabase();
	}
}

