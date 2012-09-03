using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CowMouse
{
    /// <summary>
    /// This class represents an idealized store-room (fungible quantities
    /// will be stored as integers, rather than annoying blocks).
    /// </summary>
    public class ResourceTracker
    {
        private Dictionary<ResourceType, int> resources;

        //leave this alone, it's a fixed list of all resource types!
        private ResourceType[] resourceTypes;

        public ResourceTracker()
        {
            resources = new Dictionary<ResourceType, int>();

            resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

            foreach(ResourceType resourceType in resourceTypes)
            {
                resources[resourceType] = 0;
            }
        }

        /// <summary>
        /// Determines whether or not there are enough resources to
        /// meet the specified costs.
        /// </summary>
        /// <param name="costs"></param>
        /// <returns></returns>
        public bool CanAfford(Dictionary<ResourceType, int> costs)
        {
            foreach(ResourceType value in resourceTypes)
            {
                if (costs.ContainsKey(value) && costs[value] > resources[value])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether or not there are enough resources to
        /// meet the specified costs.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool CanAfford(ResourceType resourceType, int amount)
        {
            return resources[resourceType] >= amount;
        }

        /// <summary>
        /// Spends the resources, if possible.  If not possible,
        /// no spending occurs.
        /// </summary>
        /// <param name="costs"></param>
        /// <returns>Whether or not spending occurred.</returns>
        public bool SafeSpend(Dictionary<ResourceType, int> costs)
        {
            if (CanAfford(costs))
            {
                UnsafeSpend(costs);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Spends the resources, if possible.  If not possible,
        /// no spending occurs.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns>Whether or not spending occurred.</returns>
        public bool SafeSpend(ResourceType resourceType, int amount)
        {
            if (CanAfford(resourceType, amount))
            {
                UnsafeSpend(resourceType, amount);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Spends resources whether or not they exist.
        /// Deficit spending is possible.
        /// </summary>
        /// <param name="costs"></param>
        public void UnsafeSpend(Dictionary<ResourceType, int> costs)
        {
            foreach (ResourceType value in resourceTypes)
            {
                if (costs.ContainsKey(value))
                    resources[value] -= costs[value];
            }
        }

        /// <summary>
        /// Spends resources whether or not they exist.
        /// Deficit spending is possible.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        public void UnsafeSpend(ResourceType resourceType, int amount)
        {
            resources[resourceType] -= amount;
        }

        /// <summary>
        /// Receives income of the specified types and amounts.
        /// </summary>
        /// <param name="income"></param>
        public void GetIncome(Dictionary<ResourceType, int> income)
        {
            foreach (ResourceType value in resourceTypes)
            {
                if (income.ContainsKey(value))
                    resources[value] += income[value];
            }
        }

        /// <summary>
        /// Receives income of the specified type and amount.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        public void GetIncome(ResourceType resourceType, int amount)
        {
            resources[resourceType] += amount;
        }

        /// <summary>
        /// Removes all the resources from the parameter tracker and
        /// moves them here (note this includes debt).
        /// </summary>
        /// <param name="toBeAbsorbed"></param>
        public void AbsorbAll(ResourceTracker toBeAbsorbed)
        {
            Dictionary<ResourceType, int> otherHoldings = toBeAbsorbed.CurrentHoldings();

            GetIncome(otherHoldings);
            toBeAbsorbed.UnsafeSpend(otherHoldings);
        }

        /// <summary>
        /// Returns the total amount of the specified resource that
        /// this ResourceTracker has.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int CurrentHoldings(ResourceType value)
        {
            return resources[value];
        }

        /// <summary>
        /// Returns the total amount of every resource that this ResourceTracker
        /// has.  This is a safe copy; altering any entry in here will NOT alter
        /// the amounts in the ResourceTracker.
        /// </summary>
        /// <returns></returns>
        public Dictionary<ResourceType, int> CurrentHoldings()
        {
            Dictionary<ResourceType, int> output = new Dictionary<ResourceType, int>();

            foreach (ResourceType value in resourceTypes)
            {
                output[value] = resources[value];
            }

            return output;
        }
    }

    public enum ResourceType
    {
        WOOD,

        MONEY
    }
}
