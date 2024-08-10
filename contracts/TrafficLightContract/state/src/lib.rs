#![no_std]

use io::*;
use gstd::prelude::*;
use primitive_types::U256;

#[metawasm]
pub mod metafns {
    pub type State = IoTrafficLightState;

    pub fn state(state: State) -> IoTrafficLightState{
        state
    }



}