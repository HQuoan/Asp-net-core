using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Entities;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _personService = new PersonsService(false);
            _countriesService = new CountriesService(false);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson
        //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public void AddPerson_NullPerson()
        {
            //Arange
            PersonAddRequest? personAddRequest = null;

            //Act 
            Assert.Throws<ArgumentNullException>(() =>
            {
                _personService.AddPerson(personAddRequest);
            });
        }

        //When we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public void AddPerson_PersonNameIsNull()
        {
            //Arange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = null };

            //Act 
            Assert.Throws<ArgumentException>(() =>
            {
                _personService.AddPerson(personAddRequest);
            });
        }

        //When we supply proper person details, it should insert the person into the persons list, and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public void AddPerson_ProperPersonDetails()
        {
            //Arange
            PersonAddRequest? personAddRequest = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "test@gmail.com",
                Address = "sample address",
                CountryId = Guid.NewGuid(),
                Gender = GenderOptions.Male,
                DateOfBirth = DateTime.Parse("2003-02-28")
            };

            //Act 
            PersonResponse person_response_from_add = _personService.AddPerson(personAddRequest);

            List<PersonResponse> persons_list = _personService.GetAllPersons();

            //Assert
            Assert.True(person_response_from_add.PersonId != Guid.Empty);

            Assert.Contains(person_response_from_add, persons_list);
        }

        #endregion

        #region GetPersonByPersonId
        // if we supply null as PersonId, it should return null as PersonResponse
        [Fact]
        public void GetPersonByPersonId_NullPersonId()
        {
            //Arange
            Guid? personId = null;

            //Act 
            PersonResponse? person_response_from_get = _personService.GetPersonByPersonId(personId);

            //Assert 
            Assert.Null(person_response_from_get);
        }

        //If we supply a valid person id , it should return the valid person details as PeronsResponse object
        [Fact]
        public void GetPersonByPersonId_WithPersonId()
        {
            //Arange
            CountryAddRequest country_request = new CountryAddRequest() { CountryName = "Canada" };

            CountryResponse country_response = _countriesService.AddCountry(country_request);

            //Act
            PersonAddRequest person_request = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "test@gmail.com",
                Address = "address",
                CountryId = country_response.CountryId,
                DateOfBirth = DateTime.Parse("2000-02-28"),
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = true
            }; 
            PersonResponse person_response_from_add = _personService.AddPerson(person_request);

            PersonResponse? person_response_from_get = _personService.GetPersonByPersonId(person_response_from_add.PersonId);

            //Assert
            Assert.Equal(person_response_from_add, person_response_from_get);
        }

        #endregion

        #region GetAllPersons
        // The GetAllPersons() should return an empty list by default
        [Fact]
        public void GetAllPersons_EmptyList()
        {
            //Act 
            List<PersonResponse> persons_from_get = _personService.GetAllPersons();

            //Assert
            Assert.Empty(persons_from_get);
        }

        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that we were added
        [Fact]
        public void GetAllPersons_AddFewPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "VN" };

            CountryResponse country_response_1 = _countriesService.AddCountry(countryAddRequest1);
            CountryResponse country_response_2 = _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest person_request_1 = new PersonAddRequest()
            {
                PersonName = "Smith",
                Email = "smiht@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of smith",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2000-02-28"),
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_request_2 = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "john@gmail.com",
                Gender = GenderOptions.Female,
                Address = "address of john",
                CountryId = country_response_2.CountryId,
                DateOfBirth = DateTime.Parse("2003-03-08"),
                ReceiveNewsLetters = false
            };
            PersonAddRequest person_request_3 = new PersonAddRequest()
            {
                PersonName = "Rahman",
                Email = "rahman@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of rahman",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2005-11-10"),
                ReceiveNewsLetters = true
            };

            List<PersonAddRequest> persons_requests = new List<PersonAddRequest> { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach(PersonAddRequest person_request in persons_requests)
            {
                PersonResponse person_response = _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach(PersonResponse person_response in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            //Act 
            List<PersonResponse> persons_list_from_get =
                _personService.GetAllPersons();

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in persons_list_from_get)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            //Assert
            foreach (PersonResponse person_response in person_response_list_from_add)
            {
                Assert.Contains(person_response, persons_list_from_get);
            }

        }

        #endregion

        #region GetFilteredPersons
        // if the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public void GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "VN" };

            CountryResponse country_response_1 = _countriesService.AddCountry(countryAddRequest1);
            CountryResponse country_response_2 = _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest person_request_1 = new PersonAddRequest()
            {
                PersonName = "Smith",
                Email = "smiht@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of smith",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2000-02-28"),
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_request_2 = new PersonAddRequest()
            {
                PersonName = "Mary",
                Email = "john@gmail.com",
                Gender = GenderOptions.Female,
                Address = "address of john",
                CountryId = country_response_2.CountryId,
                DateOfBirth = DateTime.Parse("2003-03-08"),
                ReceiveNewsLetters = false
            };
            PersonAddRequest person_request_3 = new PersonAddRequest()
            {
                PersonName = "Rahman",
                Email = "rahman@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of rahman",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2005-11-10"),
                ReceiveNewsLetters = true
            };

            List<PersonAddRequest> persons_requests = new List<PersonAddRequest> { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in persons_requests)
            {
                PersonResponse person_response = _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            //Act 
            List<PersonResponse> persons_list_from_search =
                _personService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            //Assert
            foreach (PersonResponse person_response in person_response_list_from_add)
            {
                Assert.Contains(person_response, persons_list_from_search);
            }

        }

        //First we will add few persons; and then when we will search based on person name with some search string, it should return the matching persons
        [Fact]
        public void GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "VN" };

            CountryResponse country_response_1 = _countriesService.AddCountry(countryAddRequest1);
            CountryResponse country_response_2 = _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest person_request_1 = new PersonAddRequest()
            {
                PersonName = "Smith",
                Email = "smiht@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of smith",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2000-02-28"),
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_request_2 = new PersonAddRequest()
            {
                PersonName = "Mary",
                Email = "john@gmail.com",
                Gender = GenderOptions.Female,
                Address = "address of john",
                CountryId = country_response_2.CountryId,
                DateOfBirth = DateTime.Parse("2003-03-08"),
                ReceiveNewsLetters = false
            };
            PersonAddRequest person_request_3 = new PersonAddRequest()
            {
                PersonName = "Rahman",
                Email = "rahman@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of rahman",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2005-11-10"),
                ReceiveNewsLetters = true
            };

            List<PersonAddRequest> persons_requests = new List<PersonAddRequest> { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in persons_requests)
            {
                PersonResponse person_response = _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            //Act 
            List<PersonResponse> persons_list_from_search =
                _personService.GetFilteredPersons(nameof(Person.PersonName), "ma");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            //Assert
            foreach (PersonResponse person_response in person_response_list_from_add)
            {
                if(person_response.PersonName != null)
                {
                    if(person_response.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(person_response, persons_list_from_search);
                    }
                }
             
            }

        }
        #endregion

        #region GetSortedPersons

        //When we sort based on PersonName in DESC, it should return persons list iin descending on PersonName
        [Fact]
        public void GetSortedPersons()
        {
            //Arrange
            CountryAddRequest countryAddRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest() { CountryName = "VN" };

            CountryResponse country_response_1 = _countriesService.AddCountry(countryAddRequest1);
            CountryResponse country_response_2 = _countriesService.AddCountry(countryAddRequest2);

            PersonAddRequest person_request_1 = new PersonAddRequest()
            {
                PersonName = "Smith",
                Email = "smiht@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of smith",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2000-02-28"),
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_request_2 = new PersonAddRequest()
            {
                PersonName = "Mary",
                Email = "john@gmail.com",
                Gender = GenderOptions.Female,
                Address = "address of john",
                CountryId = country_response_2.CountryId,
                DateOfBirth = DateTime.Parse("2003-03-08"),
                ReceiveNewsLetters = false
            };
            PersonAddRequest person_request_3 = new PersonAddRequest()
            {
                PersonName = "Rahman",
                Email = "rahman@gmail.com",
                Gender = GenderOptions.Male,
                Address = "address of rahman",
                CountryId = country_response_1.CountryId,
                DateOfBirth = DateTime.Parse("2005-11-10"),
                ReceiveNewsLetters = true
            };

            List<PersonAddRequest> persons_requests = new List<PersonAddRequest> { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in persons_requests)
            {
                PersonResponse person_response = _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            List<PersonResponse> allPersons = _personService.GetAllPersons();

            //Act 
            List<PersonResponse> persons_list_from_sort =
                _personService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in persons_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            person_response_list_from_add = person_response_list_from_add.OrderByDescending(c => c.PersonName).ToList(); 

            //Assert
            for(int i = 0; i< person_response_list_from_add.Count; i++)
            {
                Assert.Equal(person_response_list_from_add[i], persons_list_from_sort[i]);
            }

        }
        #endregion

        #region UpdatePerson

        //when we suplly null as PersonUpdateRequest , it should throw ArgumentNullException
        [Fact]
        public void UpdatePerson_NullPerson()
        {
            //Arange
            PersonUpdateRequest? person_update_request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act 
                _personService.UpdatePerson(person_update_request);
            });
        }

        //when we suplly invalid person id, it should throw ArgumentException
        [Fact]
        public void UpdatePerson_InvalidPersonId()
        {
            //Arange
            PersonUpdateRequest? person_update_request = new PersonUpdateRequest() { PersonId = Guid.NewGuid()};

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act 
                _personService.UpdatePerson(person_update_request);
            });
        }

        //When PersonName is null, it should throw ArgumentException
        [Fact]
        public void UpdatePerson_PersonNameIsNull()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse country_response_from_add = _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "John", CountryId = country_response_from_add.CountryId, Email = "john@example.com", Address = "address...", Gender = GenderOptions.Male };
            PersonResponse person_response_from_add = _personService.AddPerson(person_add_request);

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
            person_update_request.PersonName = null;


            //Assert
            Assert.Throws<ArgumentException>(() => {
                //Act
                _personService.UpdatePerson(person_update_request);
            });

        }


        //First, add a new person and try to update the person name and email
        [Fact]
        public void UpdatePerson_PersonFullDetailsUpdation()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse country_response_from_add = _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "John", CountryId = country_response_from_add.CountryId, Address = "Abc road", DateOfBirth = DateTime.Parse("2000-01-01"), Email = "abc@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse person_response_from_add = _personService.AddPerson(person_add_request);

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
            person_update_request.PersonName = "William";
            person_update_request.Email = "william@example.com";

            //Act
            PersonResponse person_response_from_update = _personService.UpdatePerson(person_update_request);

            PersonResponse? person_response_from_get = _personService.GetPersonByPersonId(person_response_from_update.PersonId);

            //Assert
            Assert.Equal(person_response_from_get, person_response_from_update);

        }

        #endregion

        #region DeletePerson

        //If you supply an valid PersonId, it should return true
        [Fact]
        public void DeletePerson_ValidPersonId()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "USA" };
            CountryResponse country_response_from_add = _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "Jones", Address = "address", CountryId = country_response_from_add.CountryId, DateOfBirth = Convert.ToDateTime("2010-01-01"), Email = "jones@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse person_response_from_add = _personService.AddPerson(person_add_request);


            //Act
            bool isDeleted = _personService.DeletePerson(person_response_from_add.PersonId);

            //Assert
            Assert.True(isDeleted);
        }


        //If you supply an invalid PersonId, it should return false
        [Fact]
        public void DeletePerson_InvalidPersonId()
        {
            //Act
            bool isDeleted = _personService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
        }

        #endregion
    }
}
