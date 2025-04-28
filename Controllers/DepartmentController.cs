using CollageManagementSystem.Core.Entities.department;
using CollageManagementSystem.Services;
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CollageMangmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DepartmentController : ControllerBase
    {
        private readonly IRepository<Department> _DepartmentRepository;

        private readonly IUserService _tokenService;


        public DepartmentController(IRepository<Department> userRepository, IUserService tokenService)
        {
            _tokenService = tokenService;
            _DepartmentRepository = userRepository;
        }


        [HttpGet("all")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _DepartmentRepository.GetAllAsync();
            return Ok(new
            {
                Departments = departments.Select(department => new DepResponseDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    HDDID = department.HDDID,
                    HDDName = department.GetDepartmentHDDName()
                })
            });
        }

        [HttpGet("{id:guid}")]
        [EnableRateLimiting("FixedWindowPolicy")]
        public async Task<IActionResult> GetDepartmentById(Guid id)
        {
            var department = await _DepartmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound("Department not found");
            }
            return Ok(new DepResponseDto
            {
                Id = department.Id,
                Name = department.Name,
                HDDID = department.HDDID,
                HDDName = department.GetDepartmentHDDName()
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepRequestDto department)
        {
            if (department == null)
            {
                return BadRequest();
            }

            Department newDepartment = new Department
            {
                Name = department.Name,
                HDDID = department.HDDID,
                HDD = await GetUserById(department.HDDID),
            };

            await _DepartmentRepository.AddAsync(
                newDepartment
            );
            return CreatedAtAction(nameof(GetDepartmentById), new { id = newDepartment.Id }, department);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepRequestDto department)
        {
            var existingDepartment = await _DepartmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
            {
                return NotFound();
            }

            existingDepartment.Name = department.Name ?? existingDepartment.Name;
            existingDepartment.HDDID = department.HDDID ?? existingDepartment.HDDID;

            await _DepartmentRepository.UpdateAsync(
                existingDepartment
            );
            return Ok(new DepResponseDto
            {
                Id = existingDepartment.Id,
                Name = existingDepartment.Name,
                HDDID = existingDepartment.HDDID,
                HDDName = existingDepartment.GetDepartmentHDDName()
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var department = await _DepartmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            await _DepartmentRepository.DeleteAsync(department);
            return Ok("Department deleted successfully");
        }

        private async Task<User?> GetUserById(string? hddId)
        {
            if (hddId == null)
            {
                return null;
            }
            var user = await _tokenService.GetUserById(Guid.Parse(hddId));
            return user;
        }
    }

}