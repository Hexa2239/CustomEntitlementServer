
using Newtonsoft.Json;

namespace EntitlementServer {

    public class EntitlementData {

        public static internalEntitlementData Read() {
            var content = File.ReadAllText("./srv/entitlements.json");
            var jsonContent = JsonConvert.DeserializeObject<internalEntitlementData>(content);

            return jsonContent;
        }

        public static void Write(string deviceId) {
            var content = File.ReadAllText("./srv/entitlements.json");
            var jsonContent = JsonConvert.DeserializeObject<internalEntitlementData>(content);

            jsonContent.entitlements.Add(deviceId);

            var stringJson = JsonConvert.SerializeObject(jsonContent);
            File.WriteAllText("./srv/entitlements.json", stringJson);
        }
    }

    public class internalEntitlementData {
        public List<string> entitlements;
    }
}