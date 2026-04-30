import Foundation

extension Notification.Name {
    /// Posted when `/api/chat` or other APS calls return 401; UI should return to sign-in.
    static let montySessionExpired = Notification.Name("montySessionExpired")
}
