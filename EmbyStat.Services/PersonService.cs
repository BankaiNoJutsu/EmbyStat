﻿using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Clients.Emby.Http;
using EmbyStat.Common.Converters;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Interfaces;

namespace EmbyStat.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IEmbyClient _embyClient;

        public PersonService(IPersonRepository personRepository, IEmbyClient embyClient)
        {
            _personRepository = personRepository;
            _embyClient = embyClient;
        }

        public async Task<Person> GetPersonByIdAsync(string id)
        {
            var person = _personRepository.GetPersonById(id);
            if (!person?.Synced ?? false)
            {
                var rawPerson = await _embyClient.GetPersonByNameAsync(person.Name, CancellationToken.None);
                person = PersonConverter.UpdatePerson(person, rawPerson);
                _personRepository.AddOrUpdatePerson(person);
            }

            return person;
        }
    }
}
