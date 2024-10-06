namespace Program
{
    public class Program
    {
        
        private static Dictionary<String, List<String>> productionRules = new Dictionary<String, List<String>>();
        private static String[]? rulesList;

        public static void expandProductionRules()
        {
            foreach (String prodRule in rulesList)
            {
                String[] rule = prodRule.Split("->", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                System.Diagnostics.Debug.Assert(rule.Length == 2, "rule length must be 2!");
                List<String> rhs = rule[1].Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                if (!productionRules.ContainsKey(rule[0]))
                {
                    productionRules.Add(rule[0], rhs);
                } else 
                {
                    foreach (String rightSideRule in rhs)
                    {
                        if (productionRules[rule[0]].Contains(rightSideRule))
                        {
                            continue;
                        }
                        productionRules[rule[0]].Add(rightSideRule);
                    }
                }
            }
        }

        public static void Main(String[] args) 
        {
            rulesList = File.ReadAllLines("grammar.txt");
            expandProductionRules();
    

        }
    }
}
