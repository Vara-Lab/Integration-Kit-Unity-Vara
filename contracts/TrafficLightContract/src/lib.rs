#![no_std]
use gstd::{async_main, collections::HashMap, msg, prelude::*, ActorId};
use io::*;

static mut STATE: Option<TrafficLightState> = None;

#[derive(Clone, Default)]
pub struct TrafficLightState {
    pub current_light: String,
    pub all_users: HashMap<ActorId, String>,
}

impl TrafficLightState {
    pub async fn firstmethod(&mut self) -> Result<TrafficLightEvents, TrafficLightErrors> {
        self.current_light = "Green".to_string();

        // Update your second field in state.
        self.all_users.insert(msg::source(), "Green".to_string());

        Ok(TrafficLightEvents::GreenEvent)
    }

    pub async fn secondmethod(&mut self) -> Result<TrafficLightEvents, TrafficLightErrors> {
        // Update your first field in state.
        self.current_light = "Yellow".to_string();

        // Update your second field in state.
        self.all_users.insert(msg::source(), "Yellow".to_string());

        Ok(TrafficLightEvents::YellowEvent)
    }

    pub async fn thirdmethod(&mut self) -> Result<TrafficLightEvents, TrafficLightErrors> {
        // Update your first field in state.
        self.current_light = "Red".to_string();

        // Update your second field in state.
        self.all_users.insert(msg::source(), "Red".to_string());

        // Generate your event.
        Ok(TrafficLightEvents::RedEvent)
    }
}

#[async_main]
async fn main() {
    // We load the input message
    let action: TrafficLightAction = msg::load().expect("Could not load Action");

    let state = unsafe { STATE.get_or_insert(Default::default()) };

    // We receive an action from the user and update the state. Example:
    let result = match action {
        TrafficLightAction::Green => state.firstmethod().await,
        TrafficLightAction::Yellow => state.secondmethod().await,
        TrafficLightAction::Red => state.thirdmethod().await,
    };

    msg::reply(result, 0).expect("Failed to encode or reply");
}

#[no_mangle]
extern "C" fn state() {
    let state = unsafe { STATE.take().expect("Unexpected error in taking state") };

    msg::reply::<IoTrafficLightState>(state.into(), 0).expect(
        "Failed to encode or reply with `<ContractMetadata as Metadata>::State` from `state()`",
    );
}

// Implementation of the From trait for converting CustomStruct to IoCustomStruct
impl From<TrafficLightState> for IoTrafficLightState {
    // Conversion method
    fn from(value: TrafficLightState) -> Self {
        // Destructure the CustomStruct object into its individual fields
        let TrafficLightState {
            current_light,
            all_users,
        } = value;

        // Perform some transformation on second field, cloning its elements (Warning: Just for HashMaps!!)
        let all_users = all_users.iter().map(|(k, v)| (*k, v.clone())).collect();

        // Create a new IoCustomStruct object using the destructured fields
        Self {
            current_light,
            all_users,
        }
    }
}
