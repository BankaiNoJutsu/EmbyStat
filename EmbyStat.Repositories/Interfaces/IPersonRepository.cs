﻿using System;
using System.Collections.Generic;
using EmbyStat.Common.Models;

namespace EmbyStat.Repositories.Interfaces
{
    public interface IPersonRepository
    {
        void AddRangeIfMissing(IEnumerable<Person> people);
        List<Guid> GetIds();
        Person GetPersonById(Guid id);
        void AddOrUpdatePerson(Person person);
    }
}
