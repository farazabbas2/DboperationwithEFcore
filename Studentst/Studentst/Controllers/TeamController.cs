using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studentst.Data;
using Studentst.DTOs;
using Studentst.Models;
using System.Reflection.Metadata.Ecma335;

namespace Studentst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public TeamController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        //get all records
        [HttpGet("")]
        public async Task<IActionResult> Getteam()
        {
            var result = await _appDbContext.Team.ToListAsync();
            return Ok(result);
        }



        //get by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetteamById([FromRoute] int id)
        {
            var result = await _appDbContext.Team.FindAsync(id);

            if (result == null)
            {
                return NotFound("Team not found");
                    }
            return Ok(result);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, UpdateTeamDto team)
        {

            var existingRecord = await _appDbContext.Team.FirstOrDefaultAsync(x => x.teamName == team.teamName && x.id != id);
            if (existingRecord != null) {
                return BadRequest("team is already exist");
}
            var existingTeam = await _appDbContext.Team
         .FirstOrDefaultAsync(x => x.id == id);

            if (existingTeam == null)
            {

                return NotFound("team not found");
            }
            existingTeam.teamName = team.teamName;

            await _appDbContext.SaveChangesAsync();

            return Ok(existingTeam);
        }



        [HttpPost]
        public async Task<IActionResult> CreateTeam(int id, CreateTeamDto team)
        {
            var existingTeam = await _appDbContext.Team
      .FirstOrDefaultAsync(x => x.teamName == team.teamName);

            if (existingTeam != null)
            {
                return BadRequest("Team already exists");
            }
            var newTeam = new Team
            {
                teamName = team.teamName

            };

            await _appDbContext.AddAsync(newTeam);
            await _appDbContext.SaveChangesAsync();

            return Ok(new
            {
                newTeam.id,
                newTeam.teamName,
            });


        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team=await _appDbContext.Team.FirstOrDefaultAsync(x => x.id == id);

            if (team != null)
            {
                return NotFound();
            }
            _appDbContext.Team.Remove(team);
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }
    }




    
}
