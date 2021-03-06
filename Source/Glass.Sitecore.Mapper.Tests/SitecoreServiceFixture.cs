﻿/*
   Copyright 2011 Michael Edwards
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Glass.Sitecore.Mapper.Configuration;
using Glass.Sitecore.Mapper.Tests.SitecoreServiceFixtureNS;
using Glass.Sitecore.Mapper.Configuration.Attributes;
using Glass.Sitecore.Mapper.Data;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SecurityModel;

namespace Glass.Sitecore.Mapper.Tests
{
    [TestFixture]
    public class SitecoreServiceFixture
    {
        SitecoreService _sitecore;
        Context _context;
        Database _db;

        [SetUp]
        public void Setup()
        {

            AttributeConfigurationLoader loader = new AttributeConfigurationLoader(
                new string []{"Glass.Sitecore.Mapper.Tests.SitecoreServiceFixtureNS, Glass.Sitecore.Mapper.Tests"}
                );

            _context = new Context(loader, new AbstractSitecoreDataHandler[]{new SitecoreIdDataHandler() });

            _sitecore = new SitecoreService("master");
            _db = global::Sitecore.Configuration.Factory.GetDatabase("master");
        }

        #region GetItem
        [Test]
        public void GetItem_ByPath_ReturnsItem()
        {
            //Assign
            string path =  "/sitecore/content/Glass/Test1";
            //Act
            TestClass result = _sitecore.GetItem<TestClass>(path);

            //Assert
            Assert.IsNotNull(result);
        }
        [Test]
        public void GetItem_ById_ReturnsItem()
        {
            //Assign
            Guid id = new Guid("{8A317CBA-81D4-4F9E-9953-64C4084AECCA}");

            //Act
            TestClass result = _sitecore.GetItem<TestClass>(id);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetItem_ByPath_ReturnsNullItemDoesNotExist()
        {
            //Assign
            string path = "/sitecore/content/Glass/DoesntExist";
            //Act
            TestClass result = _sitecore.GetItem<TestClass>(path);

            //Assert
            Assert.IsNull(result);
        }
        [Test]
        public void GetItem_ById_ReturnsNullItemDoesNotExist()
        {

            //Assign
            Guid id = new Guid("{99317CBA-81D4-4F9E-9953-64C4084AECC1}");
           
            //Act
            TestClass result = _sitecore.GetItem<TestClass>(id);

            //Assert
            Assert.IsNull(result);
        }
        #endregion

        #region Query

        [Test]
        public void Query_ReturnsSetOfClasses()
        {
            //Assign
            Item parent = _db.GetItem("/sitecore/content/glass");
            string query = "/sitecore/content/glass/*";

            //Act
            var result = _sitecore.Query<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.AreEqual(parent.Children.Count, result.Count());
            Assert.IsTrue(parent.Children.Cast<Item>().All(x => result.Any(y => y.Id == x.ID.Guid)));

        }

        [Test]
        public void Query_IncorrectQuery_ReturnsEmptyEnumeration()
        {
            //Assign
            string query = "/sitecore/content/glass/notthere/*";

            //Act
            var result = _sitecore.Query<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Query_ReturnsSetOfClasses_Proxies()
        {
            //Assign
            Item parent = _db.GetItem("/sitecore/content/glass");
            string query = "/sitecore/content/glass/*";

            //Act
            var result = _sitecore.Query<SitecoreServiceFixtureNS.TestClass>(query, true);

            //Asserte
            Assert.AreNotEqual(typeof(SitecoreServiceFixtureNS.TestClass), result.First().GetType());
            Assert.IsTrue(result.First() is SitecoreServiceFixtureNS.TestClass);
            Assert.AreEqual(parent.Children.Count, result.Count());
            Assert.IsTrue(parent.Children.Cast<Item>().All(x => result.Any(y => y.Id == x.ID.Guid)));

        }

        

        #endregion

        #region QuerySingle

        [Test]
        public void QuerySingle_ReturnsSingleClasses()
        {
            //Assign
            Item parent = _db.GetItem("/sitecore/content/glass");
            string query = "/sitecore/content/glass/*";

            //Act
            var result = _sitecore.QuerySingle<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.AreEqual(parent.Children[0].ID.Guid, result.Id);

        }
        [Test]
        public void QuerySingle_IncorrectQuery_ReturnsNull()
        {
            //Assign
            string query = "/sitecore/content/glass/notthere/*";

            //Act
            var result = _sitecore.QuerySingle<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.IsNull(result);
        }


        #endregion

        #region Create

        [Test]
        [ExpectedException(typeof(MapperException))]
        public void Create_NoTemplateId_ThrowsException()
        {
            //Assign
            TestClass test3 = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test1/Test3");

            using (new SecurityDisabler())
            {
                //Act
                _sitecore.Create<TestClass, TestClass>(test3, "Test4");


                //Assert
                //N/A
            }
        }

        [Test]
        public void Create_CreatesAnItem()
        {
            //Assign
            TestClass test3 = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test1/Test3");

            using (new SecurityDisabler())
            {
                //Act
                CreateClass newItem = _sitecore.Create<CreateClass, TestClass>(test3, "Test4");


                //Assert
                Item item = _db.GetItem("/sitecore/content/Glass/Test1/Test3/Test4");
                Assert.IsNotNull(item);
                Assert.AreNotEqual(item.ID, newItem.Id);

                try
                {
                    //Clean up
                    item.Delete();
                }
                catch (NullReferenceException ex)
                {
                    //this expection is thrown by Sitecore.Tasks.ItemEventHandler.OnItemDeleted
                }
            }
        }

        [Test]
        public void Create_CreatesAnItem_PrePopulates()
        {
            //Assign
            TestClass test3 = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test1/Test3");
            CreateClass preClass = new CreateClass() { SingleLineText = "some test data" };

            using (new SecurityDisabler())
            {


                //Act
                CreateClass newItem = _sitecore.Create<CreateClass, TestClass>(test3, "Test5", preClass);


                //Assert
                Item item = _db.GetItem("/sitecore/content/Glass/Test1/Test3/Test5");
                Assert.IsNotNull(item);
                Assert.AreNotEqual(item.ID, newItem.Id);
                Assert.AreEqual(preClass.SingleLineText, item["SingleLineText"]);

                try
                {
                    //Clean up
                    item.Delete();
                }
                catch (NullReferenceException ex)
                {
                    //this expection is thrown by Sitecore.Tasks.ItemEventHandler.OnItemDeleted
                }
            }
        }


        [Test]
        public void Create_CreatesAnItem_PrePopulates_NewMethod()
        {
            //Assign
            TestClass test3 = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test1/Test3");
            CreateClass preClass = new CreateClass() { SingleLineText = "some test data", Name = "Test6" };

            using (new SecurityDisabler())
            {


                //Act
                CreateClass newItem = _sitecore.Create<CreateClass, TestClass>(test3, preClass);


                //Assert
                Item item = _db.GetItem("/sitecore/content/Glass/Test1/Test3/Test6");


                Assert.AreEqual(preClass, newItem);
                Assert.AreEqual(item.ID.Guid, newItem.Id);
                Assert.IsNotNull(item);
                Assert.AreNotEqual(item.ID, newItem.Id);
                Assert.AreEqual(preClass.SingleLineText, item["SingleLineText"]);

                try
                {
                    //Clean up
                    item.Delete();
                }
                catch (NullReferenceException ex)
                {
                    //this expection is thrown by Sitecore.Tasks.ItemEventHandler.OnItemDeleted
                }
            }
        }

        #endregion

        #region Delete

        [Test]
        public void Delete_DoesDeleteItem()
        {
            //Assign
            string parentPath = "/sitecore/content/Glass";
            string itemName = "Test4";
            Item parent = _db.GetItem(parentPath);
            Guid templateId = new Guid("{1D0EE1F5-21E0-4C5B-8095-EDE2AF3D3300}");
            Item child = null;
            using (new SecurityDisabler())
            {
                 child = parent.Add(itemName, new TemplateID(new ID(templateId)));
            }

            Assert.IsNotNull(child);
            Guid childId = child.ID.Guid;

            TestClass childClass = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test4");
            Assert.IsNotNull(childClass);
            Assert.AreEqual(childId, childClass.Id);

            //Act
            using (new SecurityDisabler())
            {
                try
                {
                    _sitecore.Delete<TestClass>(childClass);
                }
                catch (NullReferenceException ex)
                {
                    //we need to catch a null reference exception raised by the Sitecore.Tasks.ItemEventHandler.OnItemDeleted
                }
            }

            //Assert

            Item check = _db.GetItem("/sitecore/content/Glass/Test4");
            Assert.IsNull(check);


        }

        #endregion
    }
    namespace SitecoreServiceFixtureNS
    {
        [SitecoreClass]
        public class TestClass
        {
            [SitecoreId]
            public virtual Guid Id { get; set; }

           
        }

        [SitecoreClass(TemplateId = "{1D0EE1F5-21E0-4C5B-8095-EDE2AF3D3300}")]
        public class CreateClass
        {
            [SitecoreId]
            public virtual Guid Id { get; set; }

            [SitecoreField]
            public virtual string SingleLineText { get; set; }

            [SitecoreInfo(SitecoreInfoType.Name)]
            public virtual string Name { get; set; }
        }
        

    }
}
