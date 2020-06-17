import { Component } from '@angular/core';

interface Player {
  title: string;
}

@Component({
  selector: 'GolfGroup-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title: 'GolfGroup.UI';
  players: Player[] = [{ title: 'Player 1' }, { title: 'Player 2' }];
}
