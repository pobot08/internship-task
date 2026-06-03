namespace service2.Services
{
    public class Service3Client
    {
        public Task<string> GetDataAsync()
        {
            var fakeResponse = """
        {
            "envelope": {
                "source_batch_id": "test-batch-001",
                "items_count": 1,
                "tokens_used": 50
            },
            "items": [
                {
                    "uid": "item-001",
                    "payload": "TestPayload",
                    "numeric_value": 42
                }
            ]
        }
        """;

            return Task.FromResult(fakeResponse);
        }
    }
}
