namespace Machete.Json

type Node =
| JsonText of Node 
| JsonValue of Node
| JsonObject of Node
| JsonMember of Node * Node
| JsonMemberList of Node * Node
| JsonArray of Node
| JsonElementList of Node * Node
| JsonToken of Token
| JsonNil