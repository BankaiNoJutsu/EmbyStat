﻿using System;
using System.Collections.Generic;
using System.Text;
using AspNetCore.Identity.LiteDB.Models;
using EmbyStat.Common.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Unit.Helpers
{
    public class FakeUserManager : UserManager<EmbyStatUser>
    {
        public FakeUserManager()
            : base(new Mock<IUserStore<EmbyStatUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<EmbyStatUser>>().Object,
                new IUserValidator<EmbyStatUser>[0],
                new IPasswordValidator<EmbyStatUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<EmbyStatUser>>>().Object)
        { }
    }
}
