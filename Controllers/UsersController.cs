using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service1.DAL;
using Service1.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;



namespace Service1.Controllers
{
    [Route("v2/api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly IHttpClientFactory _clientFactory;

        public UsersController(AppDbContext ctx, IHttpClientFactory clientFactory)
        {
            _ctx = ctx;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _ctx.Users.Include(u => u.UserSkills).ThenInclude(us => us.Skill).ToListAsync());
        }

        [HttpGet]
        [Route("{id?}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            StringBuilder skills = new StringBuilder();

            User user = await _ctx.Users.Include(u => u.UserSkills).ThenInclude(us => us.Skill).FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound();
            }


            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
            await _ctx.Users.AddAsync(user);
            await _ctx.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, user);
        }

        [HttpPut]
        [Route("{userId?}/skills/{skillId?}")]
        public async Task<IActionResult> Put(int? userId, int? skillId)
        {
            if (userId == null || skillId == null)
            {
                return BadRequest();
            }

            User user = await _ctx.Users.Include(u => u.UserSkills).ThenInclude(us => us.Skill).FirstOrDefaultAsync(u => u.Id == userId);
            Skill skill = await _ctx.Skills.FirstOrDefaultAsync(s => s.Id == skillId);

            if (user == null || skill == null)
            {
                return NotFound();
            }

            foreach (UserSkill item in user.UserSkills)
            {
                if (item.SkillId == skillId)
                {
                    return BadRequest();
                }
            }

            await _ctx.UserSkills.AddAsync(new UserSkill
            {
                UserId = (int)userId,
                SkillId = (int)skillId
            });

            await _ctx.SaveChangesAsync();

            return Ok(user);
        }
    }
}