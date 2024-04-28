#region Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Opc.Ua.UserDatabase;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

using Opc.Ua;
#endregion

namespace Technosoftware.UaStandardServer.Tests
{
    [TestFixture, Category("Server")]
    [SetCulture("en-us"), SetUICulture("en-us")]
    [Parallelizable]
    internal class LinqUserDatabaseTests
    {
        #region Test Methods

        [Test]
        public void CreateInvalidUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();

            //Act+ Assert
            Assert.Throws<ArgumentException>(
            () => usersDb.CreateUser("", "PW", new List<Role> { Role.AuthenticatedUser }));
            Assert.Throws<ArgumentException>(
            () => usersDb.CreateUser("Name", "", new List<Role> { Role.AuthenticatedUser }));
        }

        [Test]
        public void DeleteExistingUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();
            usersDb.CreateUser("TestUser", "PW", new List<Role> { Role.AuthenticatedUser });
            //Act
            var result = usersDb.DeleteUser("TestUser");
            //Assert
            Assert.True(result);
        }

        [Test]
        public void DeleteNonExistingUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();
            usersDb.CreateUser("TestUser", "PW", new List<Role> { Role.AuthenticatedUser });
            //Act
            var result = usersDb.DeleteUser("NoTestUser");
            //Assert
            Assert.False(result);
        }

        [Test]
        public void ChangePasswordOfExistingUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();
            usersDb.CreateUser("TestUser", "PW", new List<Role> { Role.AuthenticatedUser });
            //Act
            var result = usersDb.ChangePassword("TestUser", "PW", "newPW");
            var login = usersDb.CheckCredentials("TestUser", "newPW");
            var loginOldPW = usersDb.CheckCredentials("TestUser", "PW");
            //Assert
            Assert.True(result);
            Assert.True(login);
            Assert.False(loginOldPW);
        }

        [Test]
        public void ChangePasswordOfNonExistingUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();
            usersDb.CreateUser("TestUser", "PW", new List<Role> { Role.AuthenticatedUser });
            //Act
            var result = usersDb.DeleteUser("NoTestUser");
            //Assert
            Assert.False(result);
        }

        [Test]
        public void CheckPasswordOfExistingUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();
            usersDb.CreateUser("TestUser", "PW", new List<Role> { Role.AuthenticatedUser });
            //Act
            var result = usersDb.CheckCredentials("TestUser", "PW");
            var loginWrongPw = usersDb.CheckCredentials("TestUser", "newPW");
            //Assert
            Assert.True(result);
            Assert.False(loginWrongPw);
        }

        [Test]
        public void CheckPasswordOfNonExistingUser()
        {
            //Arrrange
            var usersDb = new LinqUserDatabase();
            usersDb.CreateUser("TestUser", "PW", new List<Role> { Role.AuthenticatedUser });
            //Act
            var result = usersDb.CheckCredentials("NoTestUser", "PW");
            //Assert
            Assert.False(result);
        }
        #endregion
    }
}
