-- Create the database
CREATE DATABASE demo;

-- Connect to the database (run this in psql or your client)
\c demo

-- Create the lights table
CREATE TABLE lights (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    is_on BOOLEAN NOT NULL
);

-- Insert sample data
INSERT INTO lights (name, is_on) VALUES
    ('Table Lamp', FALSE),
    ('Porch light', FALSE),
    ('Chandelier', TRUE);

using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Chatbot.Plugins
{
    public class MCPTools
    {
        // Tool 1: Cart Limit Check
        [KernelFunction("cart_limit_check")]
        [Description("Checks if the cart exceeds allowed limit")]
        public Task<string?> CartCheckAsync(string userId, string issue)
        {
            if (issue.Contains("cart", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<string?>("Cart limit reached.");
            return Task.FromResult<string?>(null);
        }

        // Tool 2: User Validity Check
        [KernelFunction("user_validity_check")]
        [Description("Checks if the user is valid")]
        public Task<string?> UserCheckAsync(string userId, string issue)
        {
            if (issue.Contains("invalid user", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<string?>("User is invalid.");
            return Task.FromResult<string?>(null);
        }

        // Tool 3: Address Status Check
        [KernelFunction("address_check")]
        [Description("Checks if the address is marked for deletion")]
        public Task<string?> AddressCheckAsync(string userId, string issue)
        {
            if (issue.Contains("address", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<string?>("Address marked for deletion.");
            return Task.FromResult<string?>(null);
        }

        // Tool 4: Credit Limit Check
        [KernelFunction("credit_limit_check")]
        [Description("Checks if the credit limit is exceeded")]
        public Task<string?> CreditCheckAsync(string userId, string issue)
        {
            if (issue.Contains("credit", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<string?>("Credit limit reached.");
            return Task.FromResult<string?>(null);
        }
    }
}
