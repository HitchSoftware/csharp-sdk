namespace Garden.ModelContextProtocol.Protocol;

/// <summary>
/// Represents the parameters used with a <see cref="NotificationMethods.InitializedNotification"/>
/// sent from the client to the server after initialization has finished.
/// </summary>
/// <remarks>
/// See the <see href="https://github.com/Garden.ModelContextProtocol/specification/blob/main/schema/">schema</see> for details.
/// </remarks>
public sealed class InitializedNotificationParams : NotificationParams;
