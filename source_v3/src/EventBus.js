//---------------- To send and receive Events among the components ----------------

import mitt from 'mitt';
const EventBus = mitt();
export default EventBus;
