
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace EntitlementServer {

    public class Program {

        public static HttpListenerRequest request;
        public static HttpListenerResponse response;

        public static void Main(string[] args) {

            Console.WriteLine("[Listener] Preparing Listener");

            HttpListener listener = new HttpListener();

            var port = File.ReadAllText("./srv/port.env");
            listener.Prefixes.Add("http://*:" + port + "/");

            listener.Start();

            Console.WriteLine("[Listener] Hosting Entitlement Registry on http://127.0.0.1:" + port + "/");

            while (true) {
                HttpListenerContext context = listener.GetContext();
                request = context.Request;
                response = context.Response;

                Console.WriteLine("[Server] Request Path: " + request.Url.AbsolutePath + " - " + request.HttpMethod);

                switch (request.Url.AbsolutePath) {
                    case "/api":
                        SendAndClose("Entitlement Server.", 200);
                        break;
                    case "/api/entitlement/check":
                        try {
                            var body = GetRequestBody<Json.CheckEntitlementRequest>();
                            var entitlementData = EntitlementData.Read();

                            if (entitlementData.entitlements.Contains(body.id)) {
                                SendAndClose("OK", 200);
                            } else {
                                SendAndClose("NOT OK", 200);
                            }
                        } catch (Exception e) {
                            SendAndClose("Internal Server Error", 500);
                        }

                        break;
                    case "/api/entitlement/add":
                        try {
                            if (request.HttpMethod != "POST") {
                                SendAndClose("Bad Request", 400);
                            } else {    
                                var body = GetRequestBody<Json.CheckEntitlementRequest>();
                                try {
                                    EntitlementData.Write(body.id);
                                } catch (Exception E) {
                                    Console.WriteLine("[Entitlement Writer] Error: " + E.Message);
                                    SendAndClose("Internal Server Error", 500);
                                }
                            }
                        } catch (Exception e) {
                                Console.WriteLine("[Entitlement Writer] Error: " + e.Message);
                                SendAndClose("Internal Server Error", 500);
                        }
                        break;
                    default:
                        SendAndClose("Not Found", 404);
                        break;
                }
            }
        }

        public static T GetRequestBody<T>()
        {
            var bodyStream = new StreamReader(request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            
            var bodyJson = JsonConvert.DeserializeObject<T>(bodyText);
            return bodyJson;
        }

        
        public static void SendAndClose(string data, int code) {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }

    public class Json {
        
        public class CheckEntitlementRequest {
            public string id;
        }
    }
}