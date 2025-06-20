﻿using BillingPocTwo.BillingData.Api.Data;
using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.BillingData.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntityAddressController : Controller
    {
        private readonly IBillingDbContext _context;

        public EntityAddressController(IBillingDbContext context)
        {
            _context = context;
        }

        // Route: GET api/EntityAddress
        [HttpGet]
        public async Task<IActionResult> GetAllEntityAddresses()
        {
            var entityAddresses = await _context.EntityAddresses.ToListAsync();
            return Ok(entityAddresses);
        }

        // Route: GET api/EntityAddress/{systemEntityCode}
        [HttpGet("{systemEntityCode}")]
        public async Task<IActionResult> GetEntityAddressesBySystemEntityCode(decimal systemEntityCode)
        {
            var entityAddresses = await _context.EntityAddresses
                .FirstOrDefaultAsync(e => e.SYSTEM_ENTITY_CODE == systemEntityCode && e.ADDRESS_TYPE == "Mailing");

            if (entityAddresses == null)
            {
                return NotFound($"No addresses found for SYSTEM_ENTITY_CODE: {systemEntityCode}");
            }

            return Ok(entityAddresses);
        }

        [HttpGet("all/{systemEntityCode}")]
        public async Task<ActionResult<List<ENTITY_ADDRESS_INFO>>> GetAllAddresses(decimal systemEntityCode)
        {
            var addresses = await _context.EntityAddresses
                .Where(a => a.SYSTEM_ENTITY_CODE == systemEntityCode)
                .ToListAsync();
            return Ok(addresses);
        }

        // Route: GET api/EntityAddress/addresses/{sourceSystemEntityCode}
        [HttpGet("addresses/{sourceSystemEntityCode}")]
        public async Task<IActionResult> GetAddressesBySourceSystemEntityCode(string sourceSystemEntityCode)
        {
            if (string.IsNullOrWhiteSpace(sourceSystemEntityCode))
            {
                return BadRequest("SOURCE_SYSTEM_ENTITY_CODE cannot be null or empty.");
            }

            // Find the corresponding SYSTEM_ENTITY_CODE
            var systemEntityCode = await _context.EntityRegisters
                .Where(e => e.SOURCE_SYSTEM_ENTITY_CODE == sourceSystemEntityCode)
                .Select(e => e.SYSTEM_ENTITY_CODE)
                .FirstOrDefaultAsync();

            if (systemEntityCode == 0)
            {
                return NotFound($"No SYSTEM_ENTITY_CODE found for SOURCE_SYSTEM_ENTITY_CODE: {sourceSystemEntityCode}");
            }

            // Use the SYSTEM_ENTITY_CODE to find addresses
            var addresses = await _context.EntityAddresses
                .Where(e => e.SYSTEM_ENTITY_CODE == systemEntityCode)
                .ToListAsync();

            if (!addresses.Any())
            {
                return NotFound($"No addresses found for SYSTEM_ENTITY_CODE: {systemEntityCode}");
            }

            return Ok(addresses);
        }

        [HttpPost("batch")]
        public async Task<IActionResult> GetEntityAddressesBySystemEntityCodes([FromBody] List<decimal> systemEntityCodes)
        {
            if (systemEntityCodes == null || !systemEntityCodes.Any())
            {
                return BadRequest("The list of SYSTEM_ENTITY_CODE values cannot be null or empty.");
            }

            var entityAddresses = await _context.EntityAddresses
                .Where(e => systemEntityCodes.Contains(e.SYSTEM_ENTITY_CODE))
                .ToListAsync();

            return Ok(entityAddresses);
        }

        // Route: POST api/EntityAddress
        [HttpPost]
        public async Task<IActionResult> CreateEntityAddress([FromBody] ENTITY_ADDRESS_INFO entityAddress)
        {
            if (entityAddress == null)
            {
                return BadRequest("Entity address cannot be null.");
            }

            await _context.EntityAddresses.AddAsync(entityAddress);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntityAddressesBySystemEntityCode),
                new { systemEntityCode = entityAddress.SYSTEM_ENTITY_CODE }, entityAddress);
        }

        // Route: PUT api/EntityAddressInfo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntityAddress(decimal id, [FromBody] ENTITY_ADDRESS_INFO updatedEntityAddress)
        {
            var existingEntityAddress = await _context.EntityAddresses.FindAsync(id);
            if (existingEntityAddress == null)
            {
                return NotFound($"Entity address with ID {id} not found.");
            }

            // Update properties
            existingEntityAddress.ADDRESS_TYPE = updatedEntityAddress.ADDRESS_TYPE;
            existingEntityAddress.FULL_NAME = updatedEntityAddress.FULL_NAME;
            existingEntityAddress.ADDRESS1 = updatedEntityAddress.ADDRESS1;
            existingEntityAddress.ADDRESS2 = updatedEntityAddress.ADDRESS2;
            existingEntityAddress.CITY = updatedEntityAddress.CITY;
            existingEntityAddress.STATE = updatedEntityAddress.STATE;
            existingEntityAddress.ZIP_CODE = updatedEntityAddress.ZIP_CODE;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Route: DELETE api/EntityAddressInfo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntityAddress(decimal id)
        {
            var entityAddress = await _context.EntityAddresses.FindAsync(id);
            if (entityAddress == null)
            {
                return NotFound($"Entity address with ID {id} not found.");
            }

            _context.EntityAddresses.Remove(entityAddress);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
