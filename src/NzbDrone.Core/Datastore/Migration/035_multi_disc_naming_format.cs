using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;
using System.IO;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(35)]
    public class multi_disc_naming_format : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("MultiDiscTrackFormat").AsString().Nullable();
            Execute.Sql("UPDATE NamingConfig SET MultiDiscTrackFormat = '{Medium Format} {medium:00}/{Artist Name} - {Album Title} - {track:00} - {Track Title}'");
            // Execute.WithConnection(ConvertConfig);
            // Delete.Column("Artists").FromTable("AlbumFolder");
            // Delete.Column("NamingConfig").FromTable("AlbumFolderFormat");
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = @"SELECT * FROM NamingConfig LIMIT 1";
                using (IDataReader namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    var separatorIndex = namingConfigReader.GetOrdinal("StandardTrackFormat");
                    var numberStyleIndex = namingConfigReader.GetOrdinal("AlbumFolderFormat");

                    while (namingConfigReader.Read())
                    {
                        var currentStandardTrackFormat = namingConfigReader.GetString(separatorIndex);
                        var currentAlbumFolderFormat = namingConfigReader.GetString(separatorIndex);

                        //Output settings
                        var mediumFolderFormat = "{Medium Format} {medium:00}";

                        var standardTrackFormat = Path.Combine(currentAlbumFolderFormat, currentStandardTrackFormat);

                        var standardMultiFormat = Path.Combine(currentAlbumFolderFormat, mediumFolderFormat, currentStandardTrackFormat);

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE NamingConfig " +
                                                     "SET StandardTrackFormat = '{0}', " +
                                                     "MultiDiscTrackFormat = '{1}'",
                                                     standardTrackFormat,
                                                     standardMultiFormat);

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
