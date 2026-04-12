using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;
using CharityFund.Entities;

namespace CharityFund.Services
{
    public class DatabaseService : IAsyncDisposable
    {
        private NpgsqlConnection _connection = null!;
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            _connection = new NpgsqlConnection(_connectionString);
            await _connection.OpenAsync();
        }

        public NpgsqlConnection GetConnection() => _connection;    //!

        #region --Children--
        public async Task<List<Child>> GetChildrenAsync()
        {
            var children = new List<Child>();
            string sql = "SELECT ID_child, Full_name, Birth_date FROM Child ORDER BY ID_child";

            using var cmd = new NpgsqlCommand(sql, _connection);
            using var reader = await cmd.ExecuteReaderAsync();

            for (; await reader.ReadAsync();)
            {
                children.Add(new Child
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    BirthDate = reader.GetDateTime(2)
                });
            }
            return children;
        }

        public async Task AddChildAsync(string fullName, DateTime birthDate)
        {
            try
            {
                string sql = "INSERT INTO Child (Full_name, Birth_date) VALUES (@name, @date)";
                using var cmd = new NpgsqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@date", DateOnly.FromDateTime(birthDate));
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении ребёнка:\n{ex.Message}", "Ошибка БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public async Task DeleteChildAsync(int id)
        {
            try
            {
                string sql = "DELETE FROM Child WHERE ID_child = @id";
                using var cmd = new NpgsqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region --Diseases--

        public async Task<List<Disease>> GetDiseasesAsync()
        {
            var diseases = new List<Disease>();
            string sql = "SELECT ID_disease, Name, Definition FROM Disease ORDER BY ID_disease";

            using var cmd = new NpgsqlCommand(sql, _connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                diseases.Add(new Disease
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Definition = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }
            return diseases;
        }

        public async Task AddDiseaseAsync(string name, string? definition)
        {
            try
            {
                string sql = "INSERT INTO Disease (Name, Definition) VALUES (@name, @definition)";
                using var cmd = new NpgsqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@definition", definition ?? (object)DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении болезни:\n{ex.Message}", "Ошибка БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public async Task DeleteDiseaseAsync(int id)
        {
            try
            {
                string sql = "DELETE FROM Disease WHERE ID_disease = @id";
                using var cmd = new NpgsqlCommand(sql, _connection);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region --отчёты--
        public async Task<NpgsqlDataReader> GetActiveFundraisingsAsync()
        {
            var fundraisings = new List<Fundraising>();
            string sql = @"SELECT * FROM get_active_fundraisings(true)";
            var cmd = new NpgsqlCommand(sql, _connection);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<NpgsqlDataReader> GetFinishedFundraisingsAsync()
        {
            var fundraisings = new List<Fundraising>();
            string sql = @"SELECT * FROM get_active_fundraisings(false)";
            var cmd = new NpgsqlCommand(sql, _connection);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<NpgsqlDataReader> GetDiseaseStatisticAsync(decimal minAvg, decimal maxAvg)
        {
            string sql = "SELECT * FROM get_disease_statistic(@p1, @p2)";
            var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@p1", minAvg);
            cmd.Parameters.AddWithValue("@p2", maxAvg);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<NpgsqlDataReader> GetForeignDonationsStatisticAsync(decimal minSum, decimal maxSum)
        {
            string sql = "SELECT * FROM get_foreign_donations_statistic(@p1, @p2)";
            var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("@p1", minSum);
            cmd.Parameters.AddWithValue("@p2", maxSum);
            return await cmd.ExecuteReaderAsync();
        }
        #endregion

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
