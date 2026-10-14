import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {buildGame} from '../src/native/build-game.mjs';
import {verifyRogueRoutes} from '../src/native/verify-rogue-routes.mjs';
import {runItemIntegrationChecks} from '../src/native/verify-item-game.mjs';
import {createIntegratedGameHarness} from '../src/native/verify-native-game.mjs';
import {verifyRoadMechanics} from '../src/native/verify-road-mechanics.mjs';
import {verifyMotionGame} from '../src/native/verify-motion-game.mjs';
import {verifyIntegratedUI} from '../src/native/verify-integrated-ui.mjs';
import {verifyMenuFlow} from '../src/native/verify-menu-flow.mjs';
export async function verifyGame() {
 const bundle=await buildGame();
 const results={cil:bundle.validation.valid,routes:await verifyRogueRoutes(bundle),items:runItemIntegrationChecks(bundle,createIntegratedGameHarness),roads:verifyRoadMechanics(bundle),motion:verifyMotionGame(bundle),ui:verifyIntegratedUI(bundle),menu:verifyMenuFlow(bundle)};
 const failed=Object.values(results).some(r=>r===false||(typeof r==='object'&&(r.failed>0||r.valid===false)));
 return {passed:!failed,results};
}
if(typeof process!=='undefined'&&process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)) {
 const report=await verifyGame();
 console.log(JSON.stringify(report,null,2));
 if(!report.passed)process.exitCode=1;
}
