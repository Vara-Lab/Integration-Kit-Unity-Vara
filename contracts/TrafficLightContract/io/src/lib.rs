#![no_std]

use gmeta::{InOut, Metadata, Out};
use gstd::{prelude::*, ActorId};

pub struct ProgramMetadata;

impl Metadata for ProgramMetadata {
    type Init = ();
    type Handle = InOut<TrafficLightAction, Result<TrafficLightEvents, TrafficLightErrors>>;
    type Others = ();
    type Reply = ();
    type Signal = ();
    type State = Out<IoTrafficLightState>;
}

#[derive(Decode, Encode, TypeInfo)]
#[codec(crate = gstd::codec)]
#[scale_info(crate = gstd::scale_info)]
pub enum TrafficLightAction {
    Green,
    Yellow,
    Red,
}

#[derive(Decode, Encode, TypeInfo)]
#[codec(crate = gstd::codec)]
#[scale_info(crate = gstd::scale_info)]
pub enum TrafficLightEvents {
    GreenEvent,
    YellowEvent,
    RedEvent,
}

#[derive(Decode, Encode, TypeInfo)]
#[codec(crate = gstd::codec)]
#[scale_info(crate = gstd::scale_info)]
pub enum TrafficLightErrors {
    ChangeStateError,
}

#[derive(Decode, Encode, TypeInfo)]
#[codec(crate = gstd::codec)]
#[scale_info(crate = gstd::scale_info)]
pub struct IoTrafficLightState {
    pub current_light: String,
    pub all_users: Vec<(ActorId, String)>,
}
