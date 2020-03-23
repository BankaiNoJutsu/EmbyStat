﻿using System;
using System.IO;
using EmbyStat.Common.Models.Settings;
using EmbyStat.Repositories.Interfaces;
using LiteDB;
using Microsoft.Extensions.Options;

namespace EmbyStat.Repositories
{
    public class DbContext : IDbContext
    {
        private readonly LiteDatabase _context;
        private readonly AppSettings _settings;

        public DbContext(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
            try
            {
                var dbPath = Path.Combine(settings.Value.Dirs.Data, settings.Value.DatabaseFile);
                //_context = new LiteDatabase($"FileName={dbPath};");
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("Can find or create LiteDb database.", ex);
            }
        }

        public LiteDatabase GetContext()
        {
            return _context;
        }

        public LiteDatabase CreateDatabaseContext()
        {
            try
            {
                var dbPath = Path.Combine(_settings.Dirs.Data, _settings.DatabaseFile);
                var database = new LiteDatabase($"FileName={dbPath}; Connection=shared");
                database.Mapper.EnumAsInteger = true;
                return database;
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("Can find or create LiteDb database.", ex);
            }
        }
    }
}
