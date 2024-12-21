// To send and receive Events among the components
import mitt from 'mitt';

const eventBus = mitt();

export default eventBus;
