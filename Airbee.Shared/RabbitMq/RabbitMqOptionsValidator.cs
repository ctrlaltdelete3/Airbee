using Microsoft.Extensions.Options;

namespace Airbee.Shared.RabbitMq;

// Validation logic is source-generated from the data annotations on RabbitMqOptions.
[OptionsValidator]
public partial class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
}
