using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ApplicationDbContext _db;

        public CountriesService(ApplicationDbContext db)
        {
            _db = db;
           
        }
        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            //Validation: countryAddRequest parameter can't be null
            if(countryAddRequest == null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest));
            }

            // Validation:CountryName can't be null
            if(countryAddRequest.CountryName == null)
            {
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }

            // Validation: CountryName can't be duplicate
            if(await _db.Countries.CountAsync(c => c.CountryName == countryAddRequest.CountryName) > 0) 
            {
                throw new ArgumentException("Given country name already exists");
            }

            // Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            // Generate CountryId
            country.CountryId = Guid.NewGuid();

            // Add country object into _countries
            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return await _db.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryByCountryId(Guid? countryId)
        {
            if (countryId == null)
            {
                return null;
            }

            Country? country_from_list = await _db.Countries.FirstOrDefaultAsync(c => c.CountryId == countryId);

            if(country_from_list == null)
            {
                return null;
            }

            return country_from_list.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream memory = new MemoryStream();
            await formFile.CopyToAsync(memory);

            int countriesInserted = 0;

            using (ExcelPackage excelPackage = new ExcelPackage(memory))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = worksheet.Dimension.Rows;
                

                for(int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(worksheet.Cells[row,1].Value);

                    if(!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;
                        if(_db.Countries.Where(c => c.CountryName == countryName).Count() == 0)
                        {
                            Country country = new Country()
                            {
                                CountryName = countryName
                            };

                            _db.Countries.Add(country);
                            await _db.SaveChangesAsync();

                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}
